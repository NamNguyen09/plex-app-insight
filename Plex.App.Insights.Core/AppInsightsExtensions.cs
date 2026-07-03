using System.Data.Common;
using Azure.Monitor.OpenTelemetry.Exporter;
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

        string cloudRoleName = configuration.GetConfigValue("CloudRoleName");
        string cloudRoleInstance = configuration.GetConfigValue("CloudRoleInstance");
        string cloudRoleVersion = configuration.GetConfigValue("CloudRoleVersion");
        bool enableSqlCommandTextInstrumentation = configuration.GetConfigBoolValue("EnableSqlCommandTextInstrumentation");
        bool hasCloudRoleSetting = !string.IsNullOrWhiteSpace(cloudRoleName) || !string.IsNullOrWhiteSpace(cloudRoleInstance);

        var otelBuilder = builder.Services.AddOpenTelemetry();

        if (hasCloudRoleSetting)
            otelBuilder.ConfigureResource(rb => rb.AddServiceResource(cloudRoleName, cloudRoleInstance, cloudRoleVersion));

        otelBuilder.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddSqlClientInstrumentation(options =>
                   {
                       if (enableSqlCommandTextInstrumentation)
                       {
                           options.EnrichWithSqlCommand = (activity, sqlCommand) =>
                           {
                               if (sqlCommand is DbCommand cmd)
                                   activity.SetTag("db.query.text", cmd.CommandText);
                           };
                       }
                   })
                   .AddAzureMonitorTraceExporter();
        });

        otelBuilder.WithMetrics(metrics =>
        {
            metrics.AddMeter("Microsoft.AspNetCore.Hosting")
                   .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                   .AddMeter("System.Net.Http")
                   .AddAzureMonitorMetricExporter();
        });

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
