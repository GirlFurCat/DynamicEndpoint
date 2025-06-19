using DynamicEndpoint.apis;
using DynamicEndpoint.Configuration;
using DynamicEndpoint.Core;
using DynamicEndpoint.EFCore;
using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Helpers;
using DynamicEndpoint.HostedServices;
using kasca.platform.gat1400.Infrastructure.HealthCheck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint
{
    public static class IServiceExtension
    {
        /// <summary>
        /// 添加动态路由服务
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDynamicEndpoint(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddHttpContextAccessor();
            Services.AddSingleton<DynamicEndpointDataSource>();
            Services.AddSingleton<RouteEntityService>();
            Services.AddSingleton<AppSetting>();
            Services.AddSingleton<DapperHelper>();
            Services.AddScoped<RouteEntity>();
            Services.AddScoped<ApiTemplate>();
            Services.AddScoped<EndpointFactoryHelper>();
            Services.AddScoped<EndpointFactory>();
            Services.AddScoped<RouteAggregateRoot>();
            Services.AddScoped<RouteService>();
            Services.AddScoped<BuilderSwaggerDoc>();

            Services.AddDbContext<dbContext>(options =>
                options.UseSqlServer(new AppSetting(configuration).WriteConnectionStr)
            );

            // 注册一个启动时执行迁移的 IHostedService 或 IStartupFilter
            Services.AddHostedService<ClearDynamicEndpointAssmeblyHostedService>();
            Services.AddHostedService<MigrationHostedService>();
            Services.AddHostedService<EndpointHostedService>();

            Services.AddHealthChecks()
                .AddCheck<ConfiguraHealthCheck>("Configura");

            return Services;
        }

        public static SwaggerUIOptions AddSwaggerByDynamicEndpoint(this SwaggerUIOptions options)
        {
            options.SwaggerEndpoint("/dynamic/swagger.json", "dynamic API");
            return options;
        }

        public static IApplicationBuilder UseDynamicEndpoint(this WebApplication app)
        {
            app.UseAddMinalAPI();
            var dynamicDataSource = app.Services.GetRequiredService<DynamicEndpointDataSource>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.DataSources.Add(dynamicDataSource);
            });
            return app;
        }
    }
}
