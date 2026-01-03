using System.Text.Json.Serialization;
using Ogx.Shared.Contracts.Cache;
using Ogx.Shared.Hosting.Microservices.Cache;
using Ogx.Shared.Hosting.Microservices.Filters;
using Ogx.Shared.Hosting.Microservices.Handlers;
using Ogx.Shared.Hosting.Microservices.Middlewares;
using Ogx.Shared.Hosting.Microservices.Workers;
using Ogx.Shared.Hosting.Workers;
using HsnSoft.Base;
using HsnSoft.Base.Application.Dtos;
using HsnSoft.Base.AspNetCore;
using HsnSoft.Base.AspNetCore.Hosting.Loader;
using HsnSoft.Base.Data;
using HsnSoft.Base.MultiTenancy;
using HsnSoft.Base.Timing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Ogx.Shared.Hosting.Microservices;

public static class MicroserviceHostExtensions
{
    public static IServiceCollection ConfigureMicroserviceHost(this IServiceCollection services, IConfiguration configuration, Type type)
    {
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

        // Set filter limit value
        LimitedDataRequestDto.MaxMaxResultCount = 100;

        services.ConfigureSharedHost(configuration);

        services.AddBaseAspNetCoreContextCollection();
        services.AddBaseAspNetCoreJsonLocalization();

        services.Configure<MicroserviceHostingSettings>(configuration.GetSection("HostingSettings"));

        services.AddControllers(options => { options.Filters.Add<RequestResponseActionFilterAttribute>(); })
            .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; })
            // Added for functional tests
            .AddApplicationPart(type.Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                var hostingSettings = new MicroserviceHostingSettings();
                configuration.Bind("HostingSettings", hostingSettings);

                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = hostingSettings.IgnoreNullValueForJsonResponse ? JsonIgnoreCondition.Always : JsonIgnoreCondition.Never;
            })
            .AddNewtonsoftJson(options =>
            {
                var hostingSettings = new MicroserviceHostingSettings();
                configuration.Bind("HostingSettings", hostingSettings);

                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                options.SerializerSettings.NullValueHandling = hostingSettings.IgnoreNullValueForJsonResponse ? NullValueHandling.Ignore : NullValueHandling.Include;
            });

        services.AddSingleton<IResponseExceptionHandler, ResponseExceptionHandler>();
        services.AddScoped<GlobalExceptionHandlerMiddleware>();

        // Service permission store worker
        services.AddSingleton<IServicePermissionProvider, DefaultServicePermissionProvider>();
        services.AddHostingRedis(configuration);
        services.AddTransient<ICachePermissionGrantRepository, CachePermissionGrantRepository>();
        services.AddHostedService<SynchServicePermissionStoreBackgroundService>();

        // Loader functionality
        services.AddTransient<IBasicLoader, AppBasicLoader>();
        services.AddTransient<IBasicDataSeeder, DefaultBasicDataSeeder>();
        services.AddHostedService<LoaderHostedService>();

        return services;
    }
}