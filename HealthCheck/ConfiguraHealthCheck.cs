using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace kasca.platform.gat1400.Infrastructure.HealthCheck;
public class ConfiguraHealthCheck(IConfiguration configuration) : IHealthCheck
{
  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    List<string> errors = new List<string>();
    try
    {
      if(!File.Exists("appsettings.json"))
        return await Task.FromResult(HealthCheckResult.Unhealthy("appsettings.json文件缺失"));

      if (string.IsNullOrEmpty(configuration.GetConnectionString("WriteConnection")))
        errors.Add("配置文件：数据库写连接缺失【ConnectionStrings:WriteConnection】");

      if (string.IsNullOrEmpty(configuration.GetConnectionString("ReadConnection")))
        errors.Add("配置文件：数据库读连接缺失【ConnectionStrings:ReadConnection】");
    }
    catch (Exception ex)
    {
      errors.Add($"配置文件自检异常;{ex.Message}");
    }
    if (errors.Count > 0)
      return await Task.FromResult(HealthCheckResult.Unhealthy(string.Join(';', errors)));

    return await Task.FromResult(HealthCheckResult.Healthy("配置文件自检通过，所有必需项均已设置完毕。"));

  }
}
