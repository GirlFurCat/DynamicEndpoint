using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DynamicEndpoint.Core
{
    public class RouteEntityService(IServiceProvider service)
    {
        private async Task<List<RouteEntity>> BuildRouteAsync(CancellationToken? token = null)
        {
            using var scope = service.CreateScope();
            var routeDb = scope.ServiceProvider.GetRequiredService<RouteAggregateRoot>();
            return await routeDb.ToListAsync();
        }

        private List<RouteEntity> _OldEntitys = new List<RouteEntity>();

        public async Task<(List<RouteEntity>, List<RouteEntity>)> CheckUpdateAsync(CancellationToken? token = null)
        {
            List<RouteEntity> newEntitys = await BuildRouteAsync(token);

            List<RouteEntity> addEntity = newEntitys.Where(x => !_OldEntitys.Any(y => y.path == x.path && y.method == x.method)).ToList();
            List<RouteEntity> removeEntity = _OldEntitys.Where(x => !newEntitys.Any(y => y.path == x.path && y.method == x.method)).ToList();

            //修改过其他参数项
            List<RouteEntity> modify = _OldEntitys.Where(x => !newEntitys.Any(y => JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y))).ToList();
            addEntity.AddRange(newEntitys.Where(x => modify.Any(y => y.path == x.path && y.method == x.method)).ToList());
            removeEntity.AddRange(modify);

            _OldEntitys.Clear();
            _OldEntitys.AddRange(newEntitys);
            return (addEntity, removeEntity);
        }

        /// <summary>
        /// 通知修改
        /// </summary>
        public async Task NotificationChangeAsync(EndpointFactory endpointFactory)
        {
            if (ChangeDataActive != null)
                ChangeDataActive.Invoke(await CheckUpdateAsync(), endpointFactory);

            var scope = service.CreateScope();
            var swaggerDoc = scope.ServiceProvider.GetRequiredService<BuilderSwaggerDoc>();
            await swaggerDoc.InvokeAsync();
        }

        public event Action<(List<RouteEntity>, List<RouteEntity>), EndpointFactory>? ChangeDataActive;
    }
}
