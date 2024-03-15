using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Plex.App.Insights.Core;

public static class AppInsightsExtensions
{
    public static IServiceCollection AddApplicationInsightsTelemetryAndProfiler(
                                    this IServiceCollection services,
                                    string cloudRoleName = "",
                                    string cloudRoleInstance = "")
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            services.AddApplicationInsightsTelemetry(); // Add this line of code to enable Application Insights.
            services.AddServiceProfiler(); // Add this line of code to enable Profiler

            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
                services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));
        }
        return services;
    }

    public static IServiceCollection AddApplicationInsightsTelemetry(
                                    this IServiceCollection services,
                                    string cloudRoleName = "",
                                    string cloudRoleInstance = "")
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            services.AddApplicationInsightsTelemetry(); // Add this to enable Application Insights.
            if (!string.IsNullOrWhiteSpace(cloudRoleName)
                || !string.IsNullOrWhiteSpace(cloudRoleInstance))
                services.AddSingleton<ITelemetryInitializer>(sp => new PlexCloudRoleNameInitializer(cloudRoleName, cloudRoleInstance));
        }
        return services;
    }
}
