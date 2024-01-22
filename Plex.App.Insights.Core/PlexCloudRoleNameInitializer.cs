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
            if (!string.IsNullOrWhiteSpace(CloudRoleName))
                telemetry.Context.Cloud.RoleName = CloudRoleName;

            if (!string.IsNullOrWhiteSpace(CloudRoleInstance))
                telemetry.Context.Cloud.RoleInstance = CloudRoleInstance;
        }
    }
}

