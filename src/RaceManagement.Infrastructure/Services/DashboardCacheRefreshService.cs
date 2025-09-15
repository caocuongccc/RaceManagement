using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaceManagement.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Services
{
    public class DashboardCacheRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DashboardCacheRefreshService> _logger;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);

        public DashboardCacheRefreshService(
            IServiceProvider serviceProvider,
            ILogger<DashboardCacheRefreshService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                    var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

                    _logger.LogInformation("Refreshing dashboard cache");

                    // Preload dashboard overview
                    await dashboardService.GetDashboardOverviewAsync();

                    // Preload system health
                    await dashboardService.GetSystemHealthAsync();

                    _logger.LogInformation("Dashboard cache refreshed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing dashboard cache");
                }

                await Task.Delay(_refreshInterval, stoppingToken);
            }
        }
    }
}
