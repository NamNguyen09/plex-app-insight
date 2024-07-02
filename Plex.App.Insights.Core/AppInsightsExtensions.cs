using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plex.Extensions.Configuration;

namespace Plex.App.Insights.Core;
public static class AppInsightsExtensions
{
    public static IServiceCollection AddAppInsights(this IServiceCollection services,
                                                    IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            services.AddApplicationInsightsTelemetry(); // Add this line of code to enable Application Insights.

            if (configuration.GetConfigBoolValue("EnableAppInsightsProfiler"))
            {
                services.AddServiceProfiler(); // Add this line of code to enable Profiler
            }

            string cloudRoleName = configuration.GetConfigValue("CloudRoleName");
            string cloudRoleInstance = configuration.GetConfigValue("CloudRoleInstance");
            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
            {
                services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));
            }

            bool enableSqlCommandTextInstrumentation = configuration.GetConfigBoolValue("EnableSqlCommandTextInstrumentation");
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = enableSqlCommandTextInstrumentation;
            });
        }

        return services;
    }

    static bool GetConfigBoolValue(this IConfiguration configuration, string key)
    {
        return configuration.GetConfigValue(key).Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }
}
