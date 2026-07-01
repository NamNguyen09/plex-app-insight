namespace Plex.App.Insights.Core;

/// <summary>
/// Obsolete. ITelemetryInitializer was removed in Microsoft.ApplicationInsights SDK 3.x.
/// Cloud role name is now set via OpenTelemetry resource attributes in AddAppInsights():
///   service.name        → Cloud Role Name in Application Insights
///   service.instance.id → Cloud Role Instance in Application Insights
/// Configure via appsettings: "CloudRoleName" and "CloudRoleInstance" keys.
/// </summary>
[Obsolete("ITelemetryInitializer was removed in SDK 3.x. Cloud role name is now configured automatically via AddAppInsights() using OpenTelemetry resource attributes. This class is no longer used.")]
public sealed class PlexCloudRoleNameInitializer
{
    public string? CloudRoleName { get; }
    public string? CloudRoleInstance { get; }

    public PlexCloudRoleNameInitializer() { }

    public PlexCloudRoleNameInitializer(string cloudRoleName, string cloudRoleInstance)
    {
        CloudRoleName = cloudRoleName;
        CloudRoleInstance = cloudRoleInstance;
    }
}
