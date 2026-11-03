using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace FussballNews.ai.Azure;

public static class AddLoggingExtension
{

    public static IServiceCollection SetTelemetryConfiguration(this IServiceCollection services)
    {
        services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
        {
            var telemetryProcessorChainBuilder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

            // app insights is by default sampling log to decrease the number of items being ingested into app insights
            // This can be useful for some data but for Trace and Exception, that is logged from the app code, we don't want any sampling.
            telemetryProcessorChainBuilder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 10, excludedTypes: "Trace;Exception");
            telemetryProcessorChainBuilder.Build();
        });

        return services;
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
                configuration.WriteTo.Console();
            }
        });

        return host;
    }

}
