# Plex.App.Insights.Core

A .NET 8 library that provides a single-call extension method to wire up OpenTelemetry-based Application Insights telemetry (traces and metrics) for ASP.NET Core applications.

## Features

- **One-line setup** — `builder.AddAppInsights()` configures everything
- **OpenTelemetry-based** — uses the modern OpenTelemetry SDK with Azure Monitor exporters
- **Tracing** — ASP.NET Core (inbound HTTP), HTTP client (outbound HTTP), and SQL client instrumentation
- **Metrics** — ASP.NET Core hosting, Kestrel server, and HTTP client metrics
- **SQL command capture** — optionally includes SQL command text in traces
- **Cloud role naming** — configures `service.name`, `service.instance.id`, and `service.version` as OpenTelemetry resource attributes
- **Safe no-op** — gracefully skips registration when `APPLICATIONINSIGHTS_CONNECTION_STRING` is not set

## Installation

```bash
dotnet add package Plex.App.Insights.Core
```

## Configuration

Set the Application Insights connection string as an environment variable:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...;IngestionEndpoint=...
```

Add optional settings to your `appsettings.json`:

```json
{
  "AppSettings": {
    "CloudRoleName": "my-service",
    "CloudRoleInstance": "instance-01",
    "CloudRoleVersion": "1.0.0",
    "EnableSqlCommandTextInstrumentation": "true"
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `CloudRoleName` | — | Sets the `service.name` resource attribute (identifies the service in Application Insights) |
| `CloudRoleInstance` | — | Sets the `service.instance.id` resource attribute |
| `CloudRoleVersion` | — | Sets the `service.version` resource attribute |
| `EnableSqlCommandTextInstrumentation` | `false` | When `true`, captures SQL command text (`db.query.text`) in traces |

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddAppInsights();

var app = builder.Build();
```

This registers:

**Tracing:**
- ASP.NET Core instrumentation (inbound HTTP requests)
- HTTP client instrumentation (outbound HTTP calls)
- SQL client instrumentation (with optional command text capture)
- Azure Monitor trace exporter

**Metrics:**
- `Microsoft.AspNetCore.Hosting` (request metrics)
- `Microsoft.AspNetCore.Server.Kestrel` (server metrics)
- `System.Net.Http` (HTTP client metrics)
- Azure Monitor metric exporter

If `APPLICATIONINSIGHTS_CONNECTION_STRING` is not set, the method is a no-op — safe to call in all environments.

## Dependencies

| Package | Version |
|---------|---------|
| Azure.Monitor.OpenTelemetry.Exporter | 1.8.x |
| OpenTelemetry.Instrumentation.AspNetCore | 1.17.x |
| OpenTelemetry.Instrumentation.Http | 1.17.x |
| OpenTelemetry.Instrumentation.SqlClient | 1.17.x |
| Plex.Extensions.Configuration | 8.0.x |

## License

[Plex-Solution Community Source-Available License](LICENSE.md) — free for non-commercial use only.
