﻿using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using ArtGallery.Infrastructure.Data;
using ArtGallery.Infrastructure.Repositories;
using ArtGallery.Infrastructure.Services;
using ArtGallery.Domain.Interfaces;
using ArtGallery.Application.Interfaces;
using ArtGallery.Application.Services;
using ArtGallery.Application.Mappings;
using ArtGallery.Application.Validators;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Caching.Memory;

namespace ArtGallery.API.Extensions;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the DI container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application Services
        services.AddScoped<IArtworkService, ArtworkService>();
        services.AddScoped<IExhibitionService, ExhibitionService>();
        services.AddScoped<IVisitorService, VisitorService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IInsuranceService, InsuranceService>();
        services.AddScoped<IRestorationService, RestorationService>();
        services.AddScoped<IEtlService, EtlService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the DI container with Oracle database support.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // OLTP Database Context (Oracle)
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseOracle(
                configuration.GetConnectionString("OltpConnection"),
                oracleOptions =>
                {
                    oracleOptions.CommandTimeout(configuration.GetValue<int>("Oracle:CommandTimeout", 60));
                    // Enable connection pooling settings via connection string
                });
            
            // Enable detailed errors in development
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        // DW Database Context (Oracle)
        services.AddDbContext<DwDbContext>(options =>
        {
            options.UseOracle(
                configuration.GetConnectionString("DwConnection"),
                oracleOptions =>
                {
                    oracleOptions.CommandTimeout(configuration.GetValue<int>("Oracle:CommandTimeout", 60));
                });
            
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Infrastructure Services (Oracle-specific)
        services.AddScoped<IDwAnalyticsService, DwAnalyticsService>();
        services.AddScoped<IOracleProcedureService, OracleProcedureService>();

        return services;
    }

    /// <summary>
    /// Adds AutoMapper configuration.
    /// </summary>
    public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        return services;
    }

    /// <summary>
    /// Adds FluentValidation configuration.
    /// </summary>
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateArtworkValidator>();
        return services;
    }

    /// <summary>
    /// Adds CORS configuration for Vue.js frontend.
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowVueApp", builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "https://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Adds Swagger/OpenAPI configuration.
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Adds rate limiting configuration.
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Load rate limiting settings from configuration
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    /// <summary>
    /// Adds caching configuration.
    /// </summary>
    public static IServiceCollection AddCachingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddResponseCaching();
        
        // Configure caching options
        services.Configure<MemoryCacheOptions>(options =>
        {
            options.SizeLimit = 1024; // 1024 entries max
        });

        return services;
    }
}
