using FussballNews.ai.Azure;
using FussballNews.ai.Model;
using FussballNews.ai.OpenAi.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Config.Outbox;
using Rebus.Pipeline;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.SqlServer.Outbox;
using Serilog;
using System.Diagnostics;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder, 
        string? appInsightsConnectionString = null,
        string? appConfigEndpoint = null
    )
    {
        builder.AddLogging(appInsightsConnectionString);

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.AddAppAzureAppConfiguration(appConfigEndpoint);

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder, string? appInsightsConnectionString = null)
    {
        Log.Logger = new LoggerConfiguration()
            .CreateBootstrapLogger();

        Serilog.Debugging.SelfLog.Enable(msg => {
            Debug.WriteLine(msg);
        });
        appInsightsConnectionString = string.IsNullOrWhiteSpace(appInsightsConnectionString) ? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") : appInsightsConnectionString;
        
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            builder.Services.SetTelemetryConfiguration();
            
            // Send telemetry data to Application Insights
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
                options.DeveloperMode = builder.Environment.IsDevelopment();
                options.EnableDebugLogger = builder.Environment.IsDevelopment();
                // Adaptive sampling is configured in SetTelemetryConfiguration
                options.EnableAdaptiveSampling = false;
            });
        }

        return builder;
    }


    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            var telemetryConfiguration = services.GetService<IOptions<TelemetryConfiguration>>();
            if (!string.IsNullOrEmpty(telemetryConfiguration?.Value.ConnectionString))
            {
                configuration
                    .WriteTo
                    .ApplicationInsights(telemetryConfiguration.Value, TelemetryConverter.Traces, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
            }
            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.WriteTo.Console(
                    //outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                );
            }
        });

        return host;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static IHostApplicationBuilder AddRebus(
        this IHostApplicationBuilder builder,
        string serviceBusConnectionString,
        string databaseConnectionString,
        string queueName,
        Func<IBus, Task>? onCreated = null
    )
    {
        builder.Services.AddRebus(
            configure => configure
                .Transport(t => 
                    t.UseAzureServiceBus(serviceBusConnectionString, queueName)
                    .AutomaticallyRenewPeekLock()
                )
                .Routing(r => r.TypeBased())
                .Outbox(o => o.StoreInSqlServer(databaseConnectionString, "Outbox"))
                .Options(o => {
                    o.SetNumberOfWorkers(1);
                    o.SetMaxParallelism(1);
                    o.RetryStrategy(maxDeliveryAttempts: 1);
                }),
            onCreated: onCreated
        );

        return builder;
    }

    record CurrentConnection(SqlConnection? Connection, SqlTransaction? Transaction);
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<FussballNewsAiDbContext>(provider =>
            {
                // if the FussballNewsAiDbContext is being resolved as part of the HTTP request pipeline, we get the connection like this
                CurrentConnection? GetCurrentConnetionFromHttpContext()
                {
                    var httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var items = httpContext?.Items;
                    if (items == null) {
                        return null;
                    }
                    var connection = items["current-db-connection"] as SqlConnection;
                    var transaction = items["current-db-transaction"] as SqlTransaction;
                    return new CurrentConnection(connection, transaction);
                }

                // if the FussballNewsAiDbContext is being resolved beacause Rebus is handling the message, we get the connection like this
                CurrentConnection? GetCurrentConnectionFromRebusMessageContext()
                {
                    var transactionContext = MessageContext.Current?.TransactionContext;
                    var outboxConnection = transactionContext?.Items["current-outbox-connection"] as OutboxConnection;
                    if (outboxConnection == null) {
                        return null;
                    } 
                    return new CurrentConnection(outboxConnection.Connection, outboxConnection.Transaction);
                }

                CurrentConnection GetCurrentConnection() => GetCurrentConnetionFromHttpContext()
                                                            ?? GetCurrentConnectionFromRebusMessageContext();

                var currentConnection = GetCurrentConnection();
                //if (currentConnection.Connection == null) {
                //    throw new InvalidOperationException("No connection found");
                //}

                var builder = new DbContextOptionsBuilder<FussballNewsAiDbContext>();
                if (currentConnection?.Connection == null)
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    builder.UseSqlServer(configuration.GetConnectionString("DbContext"), options => {
                        options.EnableRetryOnFailure();
                        options.UseVectorSearch();
                    });
                    return new FussballNewsAiDbContext(builder.Options);
                }
                else
                {
                    builder.UseSqlServer(currentConnection.Connection, options => {
                        options.UseVectorSearch();
                    });

                    var context = new FussballNewsAiDbContext(builder.Options);
                    context.Database.UseTransaction(currentConnection.Transaction);

                    return context;
                }
            });


        return builder;
    }

    public static IHostApplicationBuilder AddAzureStorage(this IHostApplicationBuilder builder, string storageConnectionString)
    {
        builder.Services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(storageConnectionString, preferMsi: true);
        });
        return builder;
    }

    public static IHostApplicationBuilder AddFussballNewsOpenAi(this IHostApplicationBuilder builder, Action<AiOptions> configureOptions)
    {
        builder.Services.AddFussballNewsOpenAi(configureOptions);
        return builder;
    }

    public static IHostApplicationBuilder AddAppAzureAppConfiguration(this IHostApplicationBuilder builder, string? appConfigEndpoint = null)
    {
        appConfigEndpoint = string.IsNullOrWhiteSpace(appConfigEndpoint) ? Environment.GetEnvironmentVariable("APP_CONFIG_ENDPOINT") : appConfigEndpoint;
        builder.Configuration.AddAzureAppConfiguration(appConfigEndpoint, builder.Configuration["AzureAppConfig:Label"]);
        return builder;
    }
}
