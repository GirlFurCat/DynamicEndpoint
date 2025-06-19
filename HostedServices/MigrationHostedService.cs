using DynamicEndpoint.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint.HostedServices
{
    public class MigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _provider;

        public MigrationHostedService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 创建作用域
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<dbContext>();
            try
            {
                // 只有当存在未应用的迁移时才执行 Migrate
                var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pending.Any())
                {
                    await db.Database.MigrateAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // 记录或抛出，视需求决定
                // 这里抛出会导致应用启动失败
                throw new InvalidOperationException("数据库自动迁移失败", ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
