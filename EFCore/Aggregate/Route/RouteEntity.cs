using DynamicEndpoint.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DynamicEndpoint.EFCore.Aggregate.Route
{
    /// <summary>
    /// 路由实体
    /// </summary>
    public class RouteEntity
    {
        public required int id { get; set; }
        public required string path { get; set; }

        public required string method { get; set; }

        public required string sql { get; set; }

        public required Dictionary<string, string> parameter { get; set; }

        public RouteResponse response { get; set; }

        public required bool authorization { get; set; }

        public required string version { get; set; }

        public required string introduction { get; set; }

        public required string createdBy { get; set; }

        public required DateTime createdAt { get; set; }
    }

    /// <summary>
    /// 路由实体
    /// </summary>
    public class RouteAggregateRoot(RouteEntityService service, dbContext routeDB) : AggregateRoot<RouteEntity>(routeDB)
    {
        public int Add(RouteEntity routeEntity)
        {
            _Entitys.Add(routeEntity);
            return routeEntity.id;
        }

        /// <summary>
        /// 通知修改
        /// </summary>
        public async Task NotificationChangeAsync(EndpointFactory endpointFactory) => await service.NotificationChangeAsync(endpointFactory);
    }
}
