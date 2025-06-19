using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Models.dto;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DynamicEndpoint.apis;

public class RouteService(RouteAggregateRoot routeAggregate)
{    
    public async Task<bool> AddRouteAsync(RouteEntityDto routeEntityDto)
    {
        RouteEntity routeEntity = CheckChange(routeEntityDto);
        routeAggregate.Add(routeEntity);
        await routeAggregate.Insert();
        return await routeAggregate.SaveChangesAsync();
    }

    public async Task<bool> DeleteRouteAsync(int id)
    {
        var routeEntity = await routeAggregate.AsQueryable(x=>x.id == id).FirstAsync();
        if (routeEntity == null) return false;
        routeAggregate.Add(routeEntity);
        routeAggregate.Delete();
        return await routeAggregate.SaveChangesAsync();
    }

    public async Task<bool> UpdateRouteAsync(RouteEntityDto routeEntityDto)
    {
        var entity = await routeAggregate.AsQueryable(x => x.id == routeEntityDto.id).FirstAsync();
        if (entity == null) return false;
        entity = CheckChange(routeEntityDto, entity);
        routeAggregate.Add(entity);
        routeAggregate.Update();
        return await routeAggregate.SaveChangesAsync();
    }

    /// <summary>
    /// dto修改过的参数映射到实体
    /// </summary>
    /// <param name="routeEntityDto"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    private RouteEntity CheckChange(RouteEntityDto routeEntityDto, RouteEntity? entity = null)
    {
        if (entity == null)
            entity = new RouteEntity() { id = 0, authorization = false, createdAt = DateTime.Now, createdBy = string.Empty, introduction = string.Empty, parameter = new Dictionary<string, string>(), method = string.Empty, path = string.Empty, response = RouteResponse.list, sql = string.Empty, version = string.Empty };

        PropertyInfo[] dtoPropertys = typeof(RouteEntityDto).GetProperties();
        PropertyInfo[] entityPropertys = typeof(RouteEntity).GetProperties();
        foreach (var dtoProperty in dtoPropertys)
        {
            var entityProperty = entityPropertys.FirstOrDefault(x => x.Name == dtoProperty.Name);
            if (entityProperty == null) continue;

            var dtoValue = dtoProperty.GetValue(routeEntityDto);
            var entityValue = entityProperty.GetValue(entity);
            if (dtoValue != null && !dtoValue.Equals(entityValue))
            {
                entityProperty.SetValue(entity, dtoValue);
            }
        }
        return entity;
    }
}