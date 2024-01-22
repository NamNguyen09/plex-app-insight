using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Plex.App.Insights.Core;

public class PlexCloudRoleNameInitializer : ITelemetryInitializer
{
    public PlexCloudRoleNameInitializer()
    {
        CloudRoleName = "";
        CloudRoleInstance = "";
    }
    public PlexCloudRoleNameInitializer(string cloudRoleName, string cloudRoleInstance)
    {
        CloudRoleName = cloudRoleName;
        CloudRoleInstance = cloudRoleInstance;
    }
    public string? CloudRoleName { get; }
    public string? CloudRoleInstance { get; }
    public void Initialize(ITelemetry telemetry)
    {
        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
        {
            telemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("CLOUD_ROLE_NAME") ?? CloudRoleName;
            telemetry.Context.Cloud.RoleInstance = Environment.GetEnvironmentVariable("CLOUD_ROLE_INSTANCE") ?? CloudRoleInstance;
        }
    }
}

