using System.Reflection;
using System.Security.Cryptography;
using Ogx.Shared.Helper;
using Ogx.Shared.Hosting.HealthChecks;
using Ogx.Shared.Hosting.Limiter;
using Ogx.Shared.Hosting.Middlewares;
using Ogx.Shared.Localization;
using HsnSoft.Base;
using HsnSoft.Base.AspNetCore;
using HsnSoft.Base.AspNetCore.Logging;
using HsnSoft.Base.AspNetCore.Mvc.Services;
using HsnSoft.Base.AspNetCore.Security.Claims;
using HsnSoft.Base.AspNetCore.Serilog;
using HsnSoft.Base.AspNetCore.Serilog.Persistent;
using HsnSoft.Base.AspNetCore.Tracing;
using HsnSoft.Base.Authorization;
using HsnSoft.Base.Data;
using HsnSoft.Base.Domain.Repositories;
using HsnSoft.Base.EventBus;
using HsnSoft.Base.EventBus.Logging;
using HsnSoft.Base.EventBus.RabbitMQ;
using HsnSoft.Base.EventBus.RabbitMQ.Configs;
using HsnSoft.Base.EventBus.RabbitMQ.Connection;
using HsnSoft.Base.EventBus.SubManagers;
using HsnSoft.Base.Logging;
using HsnSoft.Base.MultiTenancy;
using HsnSoft.Base.Reflection;
using HsnSoft.Base.Security.Claims;
using HsnSoft.Base.Timing;
using HsnSoft.Base.Tracing;
using HsnSoft.Base.Users;
using HsnSoft.Base.Validation.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Ogx.Shared.Hosting;

public static class SharedAspNetCoreHostExtensions
{
    // All hostings
    public static IServiceCollection ConfigureSharedHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.AddEndpointsApiExplorer();

        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentPrincipalAccessor, HttpContextCurrentPrincipalAccessor>();
        services.AddScoped<IActionContextAccessor, ActionContextAccessor>();

        services.AddSingleton<IBaseLogger, BaseLogger>();
        services.AddSingleton<IFrameworkLogger, FrameworkLogger>();

        services.Configure<HostingSettings>(configuration.GetSection("HostingSettings"));
        services.AddSingleton<IRequestResponseLogger, RequestLogger>();
        services.AddScoped<RequestResponseLoggerMiddleware>();

        services.AddScoped<SearchEngineAgentMiddleware>();

        return services;
    }

    public static IServiceCollection AddMvcRazorRender(this IServiceCollection services)
    {
        services.AddScoped<IRazorRenderService, RazorRenderService>();

        return services;
    }

    public static IServiceCollection AddJwtServerAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, string audience)
    {
        services.AddSingleton<RsaSecurityKey>(_ =>
        {
            var rsa = RSA.Create();
            try
            {
                rsa.FromXmlString(File.ReadAllText(AppContext.BaseDirectory + "/public_key.xml"));
            }
            catch (IOException ioException)
            {
                throw new BaseException("You need to provide public_key.xml to use auth", ioException);
            }

            return new RsaSecurityKey(rsa);
        });

        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // For disable Account/Login redirect when unauthorized identity jwt server
            })
            .AddJwtBearer(options =>
            {
                // var jwtSecretKey = configuration["JwtAuthServer:JwtServerSecretKey"];
                // if (string.IsNullOrWhiteSpace(jwtSecretKey)) throw new InvalidOperationException("Unknown JWT Server Key");
                // var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

                var asymmetricPublicKey = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();

                options.RequireHttpsMetadata = env.IsHostProduction() && Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = audience; // Api audience
                options.IncludeErrorDetails = !env.IsHostProduction();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true, // JWTs are required to have "aud" property set for Api audience

                    ValidateIssuer = env.IsHostProduction(),
                    ValidIssuer = configuration["AuthServer:Authority"],
                    RequireExpirationTime = true, // JWTs are required to have "exp" property set
                    ValidateLifetime = true, // The "exp" will be validated
                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    // IssuerSigningKey = symmetricKey
                    IssuerSigningKey = asymmetricPublicKey,
                    ValidTypes = ["JWT"]
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services, string[] servicePermissions)
    {
        services.AddBaseAuthorizationServiceCollection();

        services.AddAuthorization(options =>
        {
            foreach (string permissionPolicyName in servicePermissions)
            {
                options.AddPolicy(permissionPolicyName, policyBuilder =>
                {
                    // policyBuilder.RequireAuthenticatedUser();
                    // policyBuilder.RequireClaim("role");
                    policyBuilder.RequireUserPermission(permissionPolicyName);
                });
            }
        });

        return services;
    }

    public static IServiceCollection AddMicroserviceUserTenantChecker(this IServiceCollection services)
    {
        services.AddBaseDataServiceCollection();
        services.AddScoped<UserTenantCheckerMiddleware>();

        return services;
    }
    public static void UseUserTenantChecker(this IApplicationBuilder app) => app.UseMiddleware<UserTenantCheckerMiddleware>();

    public static void UseLocalization(this IApplicationBuilder app, Type serviceResourceType)
    {
        EnumHelper.Configure(app.ApplicationServices.GetService<IStringLocalizerFactory>(), serviceResourceType);
        LocalizedModelValidator.Configure(app.ApplicationServices.GetService<IStringLocalizerFactory>(), [
            serviceResourceType,
            typeof(ValidationResource)
        ]);
    }

    public static IServiceCollection AddCorsSettings(this IServiceCollection services, string corsName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsName, policy =>
            {
                policy.SetIsOriginAllowed(isOriginAllowed: _ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static void UseCorsSettings(this IApplicationBuilder app, string corsName)
    {
        app.UseCors(corsName);
    }

    public static IServiceCollection AddHostingRedis(this IServiceCollection services, IConfiguration configuration)
    {
        // services.Configure<BaseDistributedCacheOptions>(options =>
        // {
        //     options.KeyPrefix = "HsNsH:";
        // });

        // var dataProtectionBuilder = services.AddDataProtection().SetApplicationName("eShop");
        // var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        // dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "eShop-Protection-Keys");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConf = ConfigurationOptions.Parse(configuration["Redis:Configuration"] ?? throw new InvalidOperationException(), true);
            redisConf.ResolveDns = true;

            // if (redisConf.EndPoints.Count > 0 && env.EnvironmentName == "Local")
            // {
            //     redisConf.ServiceName = null;
            //     redisConf.EndPoints.Clear();
            //     redisConf.EndPoints.Add("host.docker.internal:6379"); //master
            //     redisConf.TieBreaker = "";
            // }

            return ConnectionMultiplexer.Connect(redisConf);
        });

        // var connectionString = Configuration["Redis:Configuration"];
        // var multiplexer = ConnectionMultiplexer.Connect(connectionString);
        // services.AddSingleton<IConnectionMultiplexer>(sp => multiplexer);

        services.AddSingleton(typeof(IRedisRepository<>), typeof(RedisRepository<>));
        services.AddSingleton<IRequestLimitStore, RedisRequestLimitStore>();

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddRabbitMqEventBus(configuration);

        // Add All Event Handlers
        services.AddEventHandlers(assembly);

        return services;
    }

    private static void AddRabbitMqEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        // Add configuration objects
        services.Configure<RabbitMqConnectionSettings>(configuration.GetSection("RabbitMq:Connection"));
        services.Configure<RabbitMqEventBusConfig>(configuration.GetSection("RabbitMq:EventBus"));

        // Add event bus instances
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentPrincipalAccessor, HttpContextCurrentPrincipalAccessor>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<ITraceAccesor, HttpContextTraceAccessor>();
        services.AddSingleton<IEventBusLogger, EventBusLogger>();
        services.AddSingleton<IRabbitMqPersistentConnection, RabbitMqPersistentConnection>();
        services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
        services.AddSingleton<IEventBus, EventBusRabbitMq>(sp => new EventBusRabbitMq(sp));
    }

    private static void AddEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var refType = typeof(IIntegrationEventHandler);
        var types = assembly.GetTypes()
            .Where(p => refType.IsAssignableFrom(p) && p is { IsInterface: false, IsAbstract: false });

        foreach (var type in types.ToList())
        {
            services.AddTransient(type);
        }
    }

    public static void UseEventBus(this IApplicationBuilder app, Assembly assembly, Dictionary<string, ushort> eventFetchCounts = null)
    {
        var refType = typeof(IIntegrationEventHandler);
        var eventHandlerTypes = assembly.GetTypes()
            .Where(p => refType.IsAssignableFrom(p) && p is { IsInterface: false, IsAbstract: false }).ToList();

        if (eventHandlerTypes is not { Count: > 0 }) return;

        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventType = eventHandlerType.GetInterfaces().First(x => x.IsGenericType).GenericTypeArguments[0];

            ushort fetchCount = 0;
            if (eventFetchCounts is { Count: > 0 })
            {
                if (eventFetchCounts.TryGetValue(eventType.Name, out ushort eventFetchCount))
                {
                    fetchCount = eventFetchCount;
                }
            }

            eventBus.Subscribe(eventType, eventHandlerType, fetchCount < 1 ? (ushort)1 : fetchCount);
        }
    }

    public static IServiceCollection AddHostingHealthChecks(this IServiceCollection services, IConfiguration configuration, string serviceName,
        bool checkMongo = false, string mongoConnectionName = null,
        bool checkPostgresql = false, string postgresqlConnectionName = null,
        bool checkRedis = false,
        bool checkBroker = false)
    {
        string healtCheckPrefix = serviceName ?? "service";
        var serviceProvider = services.BuildServiceProvider();
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("self-check", () => HealthCheckResult.Healthy(), tags: ["dependencies"]);

        // hcBuilder.AddUrlGroup
        // (
        //     new Uri(configuration["AuthServer:Authority"] ?? throw new InvalidOperationException()),
        //     name: $"{healtCheckPrefix}-auth-check",
        //     tags: new[] { "auth" }
        // );

        if (checkMongo)
        {
            hcBuilder.AddMongoDb(_ =>
                {
                    var mongoUrl = MongoUrl.Create(configuration.GetConnectionString(mongoConnectionName ?? throw new ArgumentNullException(nameof(mongoConnectionName))) ?? throw new InvalidOperationException());
                    return new MongoClient(MongoClientSettings.FromConnectionString(mongoUrl.Url)).GetDatabase(mongoUrl.DatabaseName);
                },
                name: $"{healtCheckPrefix}-mongo-check",
                tags: ["dependencies", "database"]
            );
            // hcBuilder.AddMongoDb(
            //     mongodbConnectionString: configuration.GetConnectionString(mongoConnectionName) ?? throw new InvalidOperationException(),
            //     name: $"{healtCheckPrefix}-mongo-check",
            //     tags: new[] { "dependencies", "database" }
            // );
        }

        if (checkPostgresql)
        {
            hcBuilder.AddNpgSql(
                connectionString: configuration.GetConnectionString(postgresqlConnectionName ?? throw new ArgumentNullException(nameof(postgresqlConnectionName))) ?? throw new InvalidOperationException(),
                name: $"{healtCheckPrefix}-postgresql-check",
                tags: ["dependencies", "database"]
            );
        }

        if (checkRedis)
        {
            var redisConnector = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
            hcBuilder.AddCheck(
                instance: new CustomRedisHealthCheck(redisConnector),
                name: $"{healtCheckPrefix}-redis-check",
                tags: ["dependencies", "database"]
            );
        }

        if (checkBroker)
        {
            var rabbitMq = new RabbitMqConnectionSettings();
            configuration.Bind("RabbitMQ:Connection", rabbitMq);
            hcBuilder.AddCheck(
                instance: new RabbitMqHealthCheck(rabbitMq),
                name: $"{healtCheckPrefix}-rabbitmq-check",
                tags: ["dependencies", "broker"]
            );

            // var connectionSettings = serviceProvider.GetRequiredService<IOptions<KafkaConnectionSettings>>().Value;
            // var producerConfig = new ProducerConfig
            // {
            //     BootstrapServers = $"{connectionSettings.HostName}:{connectionSettings.Port}",
            // };
            // hcBuilder.AddKafka(producerConfig, name: $"{healtCheckPrefix}-kafka-check", tags: new[] { "dependencies" });
        }

        return services;
    }

    public static void UseHostingHealthChecks(this IApplicationBuilder app)
    {
        // app.UseHealthChecks("/StartupCheck", new HealthCheckOptions { Predicate = _ => true });
        // app.UseHealthChecks("/LivenessCheck", new HealthCheckOptions { Predicate = _ => true });
        // app.UseHealthChecks("/ReadinessCheck", new HealthCheckOptions { Predicate = _ => true });

        app.UseHealthChecks("/StartupCheck", new HealthCheckOptions { Predicate = r => r.Tags.Contains("dependencies"), ResponseWriter = CustomHealthCheckResponse });
        app.UseHealthChecks("/LivenessCheck", new HealthCheckOptions { Predicate = r => r.Name.Equals("self-check") });
        app.UseHealthChecks("/ReadinessCheck", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("dependencies"),
            //ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            ResponseWriter = CustomHealthCheckResponse
        });
    }

    private static async Task CustomHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        string result = System.Text.Json.JsonSerializer.Serialize(
            new { status = report.Status.ToString(), checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString(), exception = e.Value.Exception?.Message, duration = e.Value.Duration }) });

        await context.Response.WriteAsync(result);
    }
}