using Azure;
using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace DynamicEndpoint.Helpers
{
    public class EndpointFactoryHelper
    {
        /// <summary>
        /// 生成代码脚本
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public (string, ClassTypeModel) BuilderScript(RouteEntity route)
        {
            //将Parameter转为String
            string paraKeysStr = ParameterKeyStr(route.parameter, route.path, route.method);

            bool isRouteOrQuery = route.method.ToLower() == "get" || route.method.ToLower() == "delete";

            ClassTypeModel classType = new ClassTypeModel();
            classType.ClassName = $"{route.method}_{route.path.Replace("/", "_").Replace("{", "_").Replace("}", "_")}";
            classType.ClassCode = isRouteOrQuery ? string.Empty : GenerateDtoSource(classType.ClassName, route.parameter.Where(item => !route.path.Contains($"{{{item.Key}}}") && !IsAutoProperty(item.Value)).ToDictionary());

            var notDtoProperty = route.parameter.Where(item => (route.path.Contains($"{{{item.Key}}}") || isRouteOrQuery) && !IsAutoProperty(item.Value)).ToDictionary();
            string paraValuesStr = ParameterValueStr(notDtoProperty);
            if (!string.IsNullOrEmpty(paraValuesStr))
                paraValuesStr += ",";

            string funcPara = FuncDynamicParaStr(route.parameter, notDtoProperty, "dto");

            //获取需要执行的方法
            string method = GetMethod(route.method, route.response);

            if (!string.IsNullOrEmpty(classType.ClassCode))
            {
                if (!string.IsNullOrEmpty(paraKeysStr))
                    paraKeysStr += ",";
                paraKeysStr += "dto";

                paraValuesStr = paraValuesStr.TrimEnd(',');
                if (!string.IsNullOrEmpty(paraValuesStr))
                    paraValuesStr += $", ";
                paraValuesStr += $"{classType.ClassName},";
            }

            if (!string.IsNullOrEmpty(paraKeysStr))
                paraKeysStr += ",";

            string script = $@"(Delegate)(Func<{paraValuesStr} IHttpContextAccessor, Task<ActionResult>>)(async ({paraKeysStr} [FromServices] contextAccessor) =>{{ var user = contextAccessor.HttpContext!.User; return await apiTemplate.{method}(new {{{funcPara}}}, ""{route.sql}""); }})";

            classType.Parameter = paraKeysStr;
            classType.ParameterType = paraValuesStr;

            return (script, classType);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Method"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public HttpMethodMetadata BuilderHttpMethodMetadata(string Method)
        {
            switch (Method.ToUpper())
            {
                case "GET":
                    return new HttpMethodMetadata(new[] { HttpMethods.Get });

                case "POST":
                    return new HttpMethodMetadata(new[] { HttpMethods.Post });

                case "DELETE":
                    return new HttpMethodMetadata(new[] { HttpMethods.Delete });

                case "PUT":
                    return new HttpMethodMetadata(new[] { HttpMethods.Put });
            }
            throw new InvalidOperationException(nameof(EndpointFactory));
        }

        /// <summary>
        /// 生成默认授权策略
        /// </summary>
        public AuthorizeAttribute BuilderAuthorize => new AuthorizeAttribute();

        /// <summary>
        /// 生成默认描述
        /// </summary>
        public OpenApiOperation BuilderDescription(string method, string description, Dictionary<string, string> para, string url)
        {
            var parameters = new List<OpenApiParameter>();
            var requestBody = new OpenApiRequestBody();

            Dictionary<string, string> queryOrRoute = new();
            Dictionary<string, string> body = new();

            //路径参数放入Parameters
            var routeParameter = para.Where(x => url.Contains($"{{{x.Key}}}")).ToDictionary();
            parameters.AddRange(BuilderPathParameter(routeParameter));

            if (method.ToLower() == "get" || method.ToLower() == "delete")
            {
                foreach (var item in para)
                {
                    if (!routeParameter.Keys.Contains(item.Key))
                        queryOrRoute.TryAdd(item.Key, item.Value);
                }
                parameters.AddRange(BuilderQueryParameter(queryOrRoute));
            }

            if (method.ToLower() == "post" || method.ToLower() == "put")
            {
                foreach (var item in para)
                {
                    if (!routeParameter.Keys.Contains(item.Key))
                        body.TryAdd(item.Key, item.Value);
                }
                requestBody = BuilderRequestBody(body);
            }

            return new OpenApiOperation()
            {
                Description = description,
                Parameters = parameters,
                RequestBody = requestBody,
                Tags = new List<OpenApiTag>()
            {
                new OpenApiTag
                {
                    Name = "DynamicEndpoint",
                    Description = "动态路由生成的API"
                }
            }
            };
        }

        /// <summary>
        /// 生成参数描述
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public OpenApiParameter[] BuilderQueryParameter(Dictionary<string, string> para) => para.Select(x => new OpenApiParameter()
        {
            Name = x.Key,
            Required = x.Value.IndexOf("?") == -1,
            In = ParameterLocation.Query
        }).ToArray();

        /// <summary>
        /// 生成路径参数
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public OpenApiParameter[] BuilderPathParameter(Dictionary<string, string> para) => para.Select(x => new OpenApiParameter()
        {
            Name = x.Key,
            Required = x.Value.IndexOf("?") == -1,
            In = ParameterLocation.Path
        }).ToArray();

        /// <summary>
        /// 根据参数字典生成一个 JSON 格式的 RequestBody
        /// </summary>
        /// <param name="para">key 是参数名，value 是类型描述（如 "string", "int?"）</param>
        /// <returns>OpenApiRequestBody</returns>
        public OpenApiRequestBody BuilderRequestBody(Dictionary<string, string> para)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new List<string>();

            foreach (var p in para)
            {
                // 提取类型信息（可根据你的命名规范再扩展）
                var typeString = p.Value.Replace("?", "").ToLowerInvariant();
                var schema = new OpenApiSchema
                {
                    Type = typeString switch
                    {
                        "int" or "int32" => "integer",
                        "long" or "int64" => "integer",
                        "float" or "double" or "decimal" => "number",
                        "bool" or "boolean" => "boolean",
                        "string" => "string",
                        "datetime" or "date" => "string",
                        _ => "string" // 默认类型
                    },
                    Format = typeString switch
                    {
                        "int" => "int32",
                        "long" => "int64",
                        "float" => "float",
                        "double" => "double",
                        "decimal" => "decimal",
                        "datetime" or "date" => "date-time",
                        _ => null
                    }
                };

                properties[p.Key] = schema;

                if (!p.Value.Contains("?")) // 如果不是可空的，加到 required 中
                {
                    required.Add(p.Key);
                }
            }

            return new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = new HashSet<string>(required)
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="IsAuthorize"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public EndpointMetadataCollection endpointMetadata(RouteEntity route)
        {
            if (route.authorization)
            {
                return new EndpointMetadataCollection(BuilderHttpMethodMetadata(route.method), BuilderDescription(route.method, route.introduction, route.parameter, route.path), BuilderAuthorize);
            }
            else
            {
                return new EndpointMetadataCollection(BuilderHttpMethodMetadata(route.method), BuilderDescription(route.method, route.introduction, route.parameter, route.path));
            }
        }

        /// <summary>
        /// 将Values转化为字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string ParameterValueStr(Dictionary<string, string> parameters)
        {
            return string.Join(", ", parameters.Values.Where(x=> !IsAutoProperty(x)).Select(x => x.Split('|')[0]));
        }

        /// <summary>
        /// 将Keys转化为字符串
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ParameterKeyStr(Dictionary<string, string> parameters, string url, string method)
        {
            string result = string.Empty;
            foreach (var item in parameters.Keys)
            {
                if (url.Contains($"{{{item}}}"))
                {
                    result += $", [FromRoute] {item}";
                    continue;
                }

                if (IsAutoProperty(parameters[item]))
                    continue;

                if (method.ToLower() == "get" || method.ToLower() == "delete")
                    result += $", [FromQuery] {item}";
            }

            return !string.IsNullOrEmpty(result) ? result.Substring(2) : string.Empty;
        }

        /// <summary>
        /// 生成类DTO源码
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="className"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        private string GenerateDtoSource(string className, Dictionary<string, string> props)
        {
            if (props.Count == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"namespace DynamicEndpoint.Models {{");
            sb.AppendLine($"  public class {className}");
            sb.AppendLine("  {");
            foreach (var p in props)
            {
                string attribute = IsAutoProperty(p.Value) ? ("[FromClaim(\"" + p.Key + "\")]") : "";
                string valueStr = $"    {attribute}public {p.Value.Split('|')[0]} {p.Key} {{ get; set; }}";
                sb.AppendLine(valueStr);
            }
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }


        /// <summary>
        /// 创建方法运行时所需要的实体值，根据like判断是否支持模糊查询
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string FuncDynamicParaStr(Dictionary<string, string> parameters, Dictionary<string, string> NotDtoProperty, string dtoName)
        {
            string value = string.Join(", ", NotDtoProperty.Select(x =>
            {
                string[] para = x.Value.Split('|');
                return para.Length > 1 && para[1].ToLower() == "like" ? $@"{x.Key}={x.Key} is null ? null : $""%{{{x.Key}}}%""" : $"{x.Key}";
            }));
            if (!string.IsNullOrEmpty(dtoName))
            {
                var dtoPropertyValue = string.Join(", ", parameters.Where(item => !NotDtoProperty.ContainsKey(item.Key)).Select(item =>
                {
                    if (IsAutoProperty(item.Value))
                    {

                        string propertyName = item.Key.Contains("ClaimTypes") ? item.Key : $"\"{item.Key}\"";
                        return $"{item.Key.Replace(".", "")}=user.FindFirst({propertyName}).Value";
                    }
                    else
                    {
                        string[] para = item.Value.Split('|');
                        string propertyName = $"{dtoName}.{item.Key}";
                        return para.Length > 1 && para[1].ToLower() == "like" ? $@"{item.Key}={propertyName} is null ? null : $""%{{{propertyName}}}%""" : $"{propertyName}";
                    }
                }));
                if (!string.IsNullOrEmpty(value))
                    value += ", ";
                value += dtoPropertyValue;
            }

            return value;
        }

        /// <summary>
        /// 获取需要执行的方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string GetMethod(string method, RouteResponse response)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    return response == RouteResponse.list ? "GetPagesAsync" : "GetAsync";

                case "POST":
                    return "PostAsync";

                case "DELETE":
                    return "DeleteAsync";

                case "PUT":
                    return "PutAsync";
            }
            throw new InvalidOperationException(nameof(EndpointFactory));
        }

        /// <summary>
        /// 判断参数是否从Token解析注入
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsAutoProperty(string value)
        {
            string[] array = value.Split('|');
            if (array.Length < 3)
                return false;
            if (array[2] == "1")
                return true;
            return false;
        }
    }
}
