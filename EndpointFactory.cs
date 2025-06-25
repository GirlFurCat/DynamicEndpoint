using DynamicEndpoint.apis;
using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Helpers;
using DynamicEndpoint.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicEndpoint
{
    public class EndpointFactory(ApiTemplate apiTemplate, EndpointFactoryHelper helper, EndpointDataSource endpointDataSource)
    {
        public async Task<Endpoint> CreateAsync(RouteEntity route)
        {
            //创建地址
            var pattern = RoutePatternFactory.Parse(route.path.Replace("{version}", route.version));

            //生成脚本
            (string scriptCode, var classCode) = helper.BuilderScript(route);

            var references =new List<Assembly>() { typeof(object).Assembly, typeof(Func<>).Assembly, Assembly.GetExecutingAssembly()};
            if (!string.IsNullOrEmpty(classCode.ClassCode))
            {
                await classCode.BuildCodeAsync();
                references.Add(classCode.Assembly!);
            }

            // 执行脚本，返回委托对象
            var func = await CSharpScript.EvaluateAsync<Delegate>(scriptCode, ScriptOptions.Default
            .WithReferences(references)
            .WithImports("System",
                "Microsoft.AspNetCore.Http",
                "DynamicEndpoint.apis",
                "DynamicEndpoint.Models",
                "DynamicEndpoint.Attributes",
                "DynamicEndpoint.EndpointFilter",
                "System.Threading.Tasks",
                "System.Security.Claims",
                "Microsoft.AspNetCore.Mvc",
                "Microsoft.AspNetCore.Http",
                "DynamicEndpoint.EFCore.Aggregate"),
            globals: new RoslynGlobalsModel() { apiTemplate = apiTemplate }
            );

            //创建委托
            var request = RequestDelegateFactory.Create(func, new RequestDelegateFactoryOptions());
            var requestDelegate = request.RequestDelegate;

            //构建终结端点
            var endpoint = new RouteEndpoint(
                requestDelegate,
                routePattern: pattern,
                order: endpointDataSource.Endpoints.Count + 1,
                metadata: helper.endpointMetadata(route),
                displayName: $"{route.method}-{route.path}"                
            );

            return endpoint;
        }
    }
}