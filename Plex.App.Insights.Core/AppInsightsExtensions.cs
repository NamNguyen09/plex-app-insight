using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Plex.App.Insights.Core;
public static class AppInsightsExtensions
{
    public static IServiceCollection AddAppInsightsTelemetryAndProfiler(
                                    this IServiceCollection services,
                                    string cloudRoleName = "",
                                    string cloudRoleInstance = "",
                                    bool enableSqlCommandTextInstrumentation = false)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            services.AddAppInsightsTelemetry(); // Add this line of code to enable Application Insights.
            services.AddServiceProfiler(); // Add this line of code to enable Profiler

            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
                services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = enableSqlCommandTextInstrumentation; });
        }
        return services;
    }

    public static IServiceCollection AddAppInsightsTelemetry(
                                    this IServiceCollection services,
                                    string cloudRoleName = "",
                                    string cloudRoleInstance = "",
                                    bool enableSqlCommandTextInstrumentation = false)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            services.AddAppInsightsTelemetry(); // Add this to enable Application Insights.
            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
                services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = enableSqlCommandTextInstrumentation; });
        }
        return services;
    }
}
