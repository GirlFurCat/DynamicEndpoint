using DynamicEndpoint;
using DynamicEndpoint.apis;
using DynamicEndpoint.Core;
using DynamicEndpoint.Models.dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DynamicEndpoint.Configuration
{
    public static class ApplicationBuilderExtend
    {
        public static IEndpointRouteBuilder UseAddMinalAPI(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, RouteEntityDto routeEntity) =>
            {
                if (await service.AddRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithDescription("增加新路由");
            //.RequireAuthorization();

            app.MapPut("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, RouteEntityDto routeEntity) =>
            {
                if (await service.UpdateRouteAsync(routeEntity))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithDescription("更新指定路由")
            .RequireAuthorization();

            app.MapDelete("/api/admin/route", async ([FromServices] RouteService service, [FromServices] RouteEntityService routeEntityService, [FromServices] EndpointFactory endpointFactory, int id) =>
            {
                if (await service.DeleteRouteAsync(id))
                    await routeEntityService.NotificationChangeAsync(endpointFactory);
                else
                    return Results.BadRequest();
                return Results.Ok();
            }).WithDescription("删除指定路由")
            .RequireAuthorization();

            return app;
        }
    }
}
