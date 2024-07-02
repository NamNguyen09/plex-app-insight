using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Plex.Extensions.Configuration;

namespace Plex.App.Insights.Core;
public static class AppInsightsExtensions
{
    public static WebApplicationBuilder AddAppInsights(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            builder.Services.AddApplicationInsightsTelemetry(); // Add this line of code to enable Application Insights.

            var configuration = builder.Configuration;
            if (configuration.GetConfigBoolValue("EnableAppInsightsProfiler"))
            {
                builder.Services.AddServiceProfiler(); // Add this line of code to enable Profiler
            }

            string cloudRoleName = configuration.GetConfigValue("CloudRoleName");
            string cloudRoleInstance = configuration.GetConfigValue("CloudRoleInstance");
            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
            {
                builder.Services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));
            }

            bool enableSqlCommandTextInstrumentation = configuration.GetConfigBoolValue("EnableSqlCommandTextInstrumentation");
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = enableSqlCommandTextInstrumentation;
            });
        }

        return builder;
    }

    static bool GetConfigBoolValue(this IConfiguration configuration, string key)
    {
        return configuration.GetConfigValue(key).Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }
}
