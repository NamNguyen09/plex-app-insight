using System.Data.Common;
using Azure.Monitor.OpenTelemetry.AspNetCore;
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

        builder.Services.AddOpenTelemetry().UseAzureMonitor();

        string cloudRoleName = configuration.GetConfigValue("CloudRoleName");
        string cloudRoleInstance = configuration.GetConfigValue("CloudRoleInstance");
        string cloudRoleVersion = configuration.GetConfigValue("CloudRoleVersion");
        bool enableSqlCommandTextInstrumentation = configuration.GetConfigBoolValue("EnableSqlCommandTextInstrumentation");
        bool hasCloudRoleSetting = !string.IsNullOrWhiteSpace(cloudRoleName) || !string.IsNullOrWhiteSpace(cloudRoleInstance);

        builder.Services.ConfigureOpenTelemetryTracerProvider((sp, b) =>
        {
            if (hasCloudRoleSetting)
                b.ConfigureResource(rb => rb.AddServiceResource(cloudRoleName, cloudRoleInstance, cloudRoleVersion));

            b.AddSqlClientInstrumentation(options =>
            {
                if (enableSqlCommandTextInstrumentation)
                {
                    options.EnrichWithSqlCommand = (activity, sqlCommand) =>
                    {
                        if (sqlCommand is DbCommand cmd)
                            activity.SetTag("db.query.text", cmd.CommandText);
                    };
                }
            });
        });

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
