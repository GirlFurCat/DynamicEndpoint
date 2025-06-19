using DynamicEndpoint.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint.HostedServices
{
    public class ClearDynamicEndpointAssmeblyHostedService:IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 清理动态路由程序集缓存
            string[] files = Directory.GetFiles(new ClassTypeModel().AssemblyDir);
            foreach (string item in files)
                File.Delete(item);
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 在应用停止时不需要执行任何操作
            return Task.CompletedTask;
        }
    }
}
