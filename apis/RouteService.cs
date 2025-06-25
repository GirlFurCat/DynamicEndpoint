using Ardalis.Result;
using DynamicEndpoint.EFCore.Aggregate.Route;
using DynamicEndpoint.Helpers;
using DynamicEndpoint.Models.dto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DynamicEndpoint.apis;

public class RouteService(RouteAggregateRoot routeAggregate, IHttpContextAccessor contextAccessor)
{
    public async Task<Result> AddRouteAsync(RouteEntityDto routeEntityDto)
    {
        var paramCheckResult = ParamCheck(routeEntityDto);
        if (!paramCheckResult.IsSuccess) return paramCheckResult;

        RouteEntity routeEntity = CheckChange(routeEntityDto);
        routeEntity.createdAt = DateTime.Now;
        routeEntity.createdBy = contextAccessor.HttpContext?.User.Identity?.Name ?? routeEntity.createdBy; // 获取当前用户名称，若未登录则默认为传入值
        routeAggregate.Add(routeEntity);
        await routeAggregate.Insert();
        return await routeAggregate.SaveChangesAsync() ? Result.Success() : Result.Error("路由添加失败");
    }

    public async Task<Result> DeleteRouteAsync(int id)
    {
        var routeEntity = await routeAggregate.AsQueryable(x => x.id == id).FirstAsync();
        if (routeEntity == null) return Result.Error("路由不存在");
        routeAggregate.Add(routeEntity);
        routeAggregate.Delete();
        return await routeAggregate.SaveChangesAsync() ? Result.Success() : Result.Error("路由删除失败");
    }

    public async Task<Result> UpdateRouteAsync(RouteEntityDto routeEntityDto)
    {
        var paramCheckResult = ParamCheck(routeEntityDto);
        if (!paramCheckResult.IsSuccess) return paramCheckResult;

        var entity = await routeAggregate.AsQueryable(x => x.id == routeEntityDto.id).FirstAsync();
        if (entity == null) return Result.Error("路由不存在");
        entity = CheckChange(routeEntityDto, entity);
        routeAggregate.Add(entity);
        routeAggregate.Update();
        return await routeAggregate.SaveChangesAsync() ? Result.Success() : Result.Error("路由更新失败");
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

    /// <summary>
    /// 路由各项参数检查
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    private Result ParamCheck(RouteEntityDto route)
    {
        List<string> error = new List<string>();

        //地址检查：普通地址和Restful地址
        if (string.IsNullOrEmpty(route.path) || !Regex.IsMatch(route.path, @"^\/[a-zA-Z0-9\-\/{}]*$"))
            error.Add("地址[path]非法");

        if (string.IsNullOrEmpty(route.method) || !Regex.IsMatch(route.method, @"^(GET|POST|PUT|DELETE)$"))
            error.Add("请求方式[method]非法");

        if (string.IsNullOrEmpty(route.sql))
            error.Add("SQL语句[sql]非法");

        if (route.parameter is not null && route.parameter.Count > 0)
        {
            foreach (var param in route.parameter)
            {
                if (string.IsNullOrEmpty(param.Key) || string.IsNullOrEmpty(param.Value))
                {
                    error.Add($"参数[{param.Key}]或值[{param.Value}]非法");
                }

                string[] valueArray = param.Value.Split('|');
                for (int i = 0; i < valueArray.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            //参数类型非法
                            if (!AllowedNames.Any(x => x == valueArray[i]))
                                error.Add($"参数[{param.Key}]的类型非法");
                            break;
                        case 1:
                            if (!string.IsNullOrEmpty(valueArray[i]) && valueArray[i].ToLower() != "like")
                                error.Add($"参数[{param.Key}]的模糊查询标识非法");

                            if (!string.IsNullOrEmpty(valueArray[i]) && !string.IsNullOrEmpty(route.method) && route.method.ToLower() != "get")
                                error.Add($"非Get请求下，参数[{param.Key}]的模糊查询标识非法");
                            break;
                        case 2:
                            if (!string.IsNullOrEmpty(valueArray[i]) && valueArray[i].ToLower() != "claim")
                                error.Add($"参数[{param.Key}]的自动注入标识非法");
                            break;
                    }
                }
            }
        }
    
        if(route.response is null)
            error.Add("响应格式[response]非法");

        if(route.authorization is null)
            error.Add("令牌标识[authorization]非法");

        if(route.version is null)
            error.Add("版本号[version]非法");

        if (error.Count > 0)
            return Result.Error(error.ToString(';'));

        return Result.Success();
    }
    private static readonly HashSet<string> AllowedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "string",    // 对应 System.String
        "int",       // 对应 System.Int32
        "long",      // System.Int64
        "bool",      // System.Boolean
        "double",    // System.Double
        "float",     // System.Single
        "decimal",   // System.Decimal
        "DateTime",  // System.DateTime
        "Guid",      // System.Guid
        "byte",
        "short",
        "ushort",
        "uint",
        "ulong",
        "char",
        "object",
        "string?",   
        "int?",       
        "long?",      
        "bool?",      
        "double?",    
        "float?",     
        "decimal?",   
        "DateTime?",  
        "Guid?",
        "byte?",
        "short?",
        "ushort?",
        "uint?",
        "ulong?",
        "char?",
        "object?"
    };
}