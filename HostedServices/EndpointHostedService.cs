using DynamicEndpoint;
using DynamicEndpoint.apis;
using DynamicEndpoint.Configuration;
using DynamicEndpoint.Core;
using DynamicEndpoint.EFCore.Aggregate;
using DynamicEndpoint.EFCore.Aggregate.Route;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace DynamicEndpoint.HostedServices
{
    public class EndpointHostedService(DynamicEndpointDataSource _endpointDataSource, RouteEntityService routeService, IServiceProvider service) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = service.CreateScope();
            ApiTemplate apiTemplate = scope.ServiceProvider.GetRequiredService<ApiTemplate>();
            EndpointFactory endpointFactory = scope.ServiceProvider.GetRequiredService<EndpointFactory>();
            routeService.ChangeDataActive += RouteEntitys_ChangeDataActive;
            await routeService.NotificationChangeAsync(endpointFactory);
        }

        private void RouteEntitys_ChangeDataActive((List<RouteEntity> addEndpoint, List<RouteEntity> removeEndpoint) obj, EndpointFactory endpointFactory)
        {
            //移除旧端点
            obj.removeEndpoint.ForEach(x => _endpointDataSource.RemoveEndpoint($"{x.method}-{x.path}"));

            //添加新端点
            obj.addEndpoint.ForEach(async x => _endpointDataSource.AddEndpoint(await endpointFactory.CreateAsync(x)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
