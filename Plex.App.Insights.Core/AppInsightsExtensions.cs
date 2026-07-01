using System.Data.Common;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plex.Extensions.Configuration;

namespace Plex.App.Insights.Core;

public static class AppInsightsExtensions
{
    public static WebApplicationBuilder AddAppInsights(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"))) return builder;

        var configuration = builder.Configuration;

        builder.Services.AddApplicationInsightsTelemetry();

        // Note: Microsoft.ApplicationInsights.Profiler.AspNetCore is incompatible with SDK 3.x.
        // For profiling with SDK 3.x, use Azure.Monitor.OpenTelemetry.Profiler instead.

        string cloudRoleName = configuration.GetConfigValue("CloudRoleName");
        string cloudRoleInstance = configuration.GetConfigValue("CloudRoleInstance");
        string cloudRoleVersion = configuration.GetConfigValue("CloudRoleVersion");
        bool enableSqlCommandTextInstrumentation = configuration.GetConfigBoolValue("EnableSqlCommandTextInstrumentation");
        bool hasCloudRoleSetting = !string.IsNullOrWhiteSpace(cloudRoleName) || !string.IsNullOrWhiteSpace(cloudRoleInstance);

        // ITelemetryInitializer and DependencyTrackingTelemetryModule are removed in SDK 3.x.
        // Configure resource attributes on BOTH trace and log providers so cloud role name
        // appears on all telemetry (requests/dependencies AND log entries) in Application Insights.
        //   service.name        → client.Context.Cloud.RoleName
        //   service.instance.id → client.Context.Cloud.RoleInstance
        //   service.version     → client.Context.Component.Version
        builder.Services.ConfigureOpenTelemetryTracerProvider((sp, b) =>
        {
            if (hasCloudRoleSetting)
                b.ConfigureResource(rb => rb.AddServiceResource(cloudRoleName, cloudRoleInstance, cloudRoleVersion));

            // Replaces DependencyTrackingTelemetryModule.EnableSqlCommandTextInstrumentation.
            // SetDbStatementForText/SetDbStatementForStoredProcedure were removed in OpenTelemetry.Instrumentation.SqlClient 1.9+.
            // SQL query text is now captured via EnrichWithSqlCommand when enabled.
            b.AddSqlClientInstrumentation(options =>
            {
                if (enableSqlCommandTextInstrumentation)
                {
                    // To capture SQL parameters, set this in appsettings.json or environment variable: OTEL_DOTNET_EXPERIMENTAL_SQLCLIENT_ENABLE_TRACE_DB_QUERY_PARAMETERS
                    options.EnrichWithSqlCommand = (activity, sqlCommand) =>
                    {
                        // EnrichWithSqlCommand receives object — cast via DbCommand base class
                        // to support both Microsoft.Data.SqlClient and System.Data.SqlClient
                        if (sqlCommand is DbCommand cmd)
                            activity.SetTag("db.query.text", cmd.CommandText);
                    };
                }
            });
        });

        // Logs: ILogger entries, exceptions
        if (hasCloudRoleSetting)
        {
            builder.Services.ConfigureOpenTelemetryLoggerProvider((sp, b) =>
                b.ConfigureResource(rb => rb.AddServiceResource(cloudRoleName, cloudRoleInstance, cloudRoleVersion)));
        }

        return builder;
    }

    static ResourceBuilder AddServiceResource(this ResourceBuilder rb, string roleName, string roleInstance, string roleVersion)
    {
        if (!string.IsNullOrWhiteSpace(roleName))
            return rb.AddService(
                serviceName: roleName,
                serviceVersion: string.IsNullOrWhiteSpace(roleVersion) ? null : roleVersion,
                serviceInstanceId: string.IsNullOrWhiteSpace(roleInstance) ? null : roleInstance);

        return rb.AddAttributes(new Dictionary<string, object> { ["service.instance.id"] = roleInstance! });
    }

    static bool GetConfigBoolValue(this IConfiguration configuration, string key)
    {
        return configuration.GetConfigValue(key).Equals("true", StringComparison.InvariantCultureIgnoreCase);
    }
}
