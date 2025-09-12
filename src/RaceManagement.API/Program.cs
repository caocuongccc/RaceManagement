using Microsoft.EntityFrameworkCore;
using RaceManagement.Infrastructure.Data;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Repositories;
using RaceManagement.Infrastructure.Services;
using RaceManagement.Application.Services;
using RaceManagement.API.Middleware;
using Hangfire;
using RaceManagement.Application.Jobs;
using RaceManagement.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Race Management API",
        Version = "v1",
        Description = "API for managing race registrations and payments"
    });

    // Enable XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database Configuration
builder.Services.AddDbContext<RaceManagementDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

// Configure Email Services
builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection("EmailConfiguration"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IEmailJob, EmailJob>();
builder.Services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();
builder.Services.AddScoped<IEmailQueueProcessor, EmailQueueProcessor>();


// Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRaceRepository, RaceRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IRaceShirtTypeRepository, RaceShirtTypeRepository>();


// Google Sheets Service
builder.Services.AddScoped<IGoogleSheetsService, GoogleSheetsService>();

// Application Services
builder.Services.AddScoped<IRaceService, RaceService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();

// Application Services
builder.Services.AddScoped<ICredentialService, CredentialService>();
builder.Services.AddScoped<ISheetConfigService, SheetConfigService>();



// Configure CORS for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Configure Hangfire Dashboard
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// Setup Recurring Jobs
RecurringJob.AddOrUpdate<IEmailJob>(
    "process-pending-emails",
    x => x.ProcessPendingEmailsAsync(),
    "*/2 * * * *"); // Every 2 minutes

RecurringJob.AddOrUpdate<IEmailJob>(
    "process-scheduled-emails",
    x => x.ProcessScheduledEmailsAsync(),
    "*/5 * * * *"); // Every 5 minutes

RecurringJob.AddOrUpdate<IEmailJob>(
    "retry-failed-emails",
    x => x.RetryFailedEmailsAsync(),
    "0 */6 * * *"); // Every 6 hours

RecurringJob.AddOrUpdate<IEmailJob>(
    "cleanup-old-email-logs",
    x => x.CleanupOldEmailLogsAsync(30),
    "0 2 * * *"); // Daily at 2 AM

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Race Management API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("DevelopmentPolicy");
}
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();