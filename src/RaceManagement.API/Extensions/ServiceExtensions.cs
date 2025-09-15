using RaceManagement.Application.Jobs;
using RaceManagement.Application.Services;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Repositories;
using RaceManagement.Infrastructure.Services;

namespace RaceManagement.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDashboardServices(this IServiceCollection services)
        {
            // Dashboard services
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // Memory cache for dashboard
            services.AddMemoryCache();

            // Background services
            services.AddHostedService<DashboardCacheRefreshService>();

            return services;
        }

        public static IServiceCollection AddEmailServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IEmailQueueProcessor, EmailQueueProcessor>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailJob, EmailJob>();
            // FIX: Add missing QRCode service
            services.AddScoped<IQRCodeService, QRCodeService>();

            return services;
        }

        public static IServiceCollection AddRaceManagementServices(this IServiceCollection services)
        {
            // Core services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRaceService, RaceService>();
            services.AddScoped<IRegistrationService, RegistrationService>();

            // Repositories
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<IRaceRepository, RaceRepository>();
            services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();

            // FIX: Add missing repositories from your old Program.cs
            services.AddScoped<IRaceShirtTypeRepository, RaceShirtTypeRepository>();

            // Add other services from old Program.cs
            services.AddScoped<ICredentialService, CredentialService>();
            services.AddScoped<ISheetConfigService, SheetConfigService>();

            return services;
        }
    }
}
