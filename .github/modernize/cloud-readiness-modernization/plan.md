# Modernization Plan: Cloud Readiness Modernization

**Project**: TubieTools_Aspire

---

## Technical Framework

- **Language**: C# (.NET 10.0)
- **Framework**: ASP.NET Core / .NET Aspire (multi-project solution, 21 projects)
- **Build Tool**: MSBuild / dotnet CLI
- **Database**: SQL Server (connection strings detected in `DataAccessLayer/appsettings.json`)
- **Key Dependencies**: Entity Framework, Serilog

---

## Overview

> This migration addresses cloud-readiness and security findings from the AppCAT assessment (report `report-20260908143210`), scoped to 11 selected categories covering configuration, credentials, HTTP communication, file system access, performance, connection strings, local system dependencies, certificates, and logging. The application currently stores non-secret configuration and secrets (including plaintext database passwords) in local `appsettings.json` files, logs to local/network file paths via Serilog, and uses synchronous APIs and local file/process dependencies that are not portable to Azure App Service, AKS, or Azure Container Apps. The new architecture will:
>
> - Externalize non-secret configuration to Azure App Configuration (SDK-integrated, dynamic refresh) instead of local `appsettings.json`
> - Move plaintext secrets and connection strings to Azure Key Vault, authenticated via Managed Identity
> - Replace local file storage, local file logging, and Serilog file sinks with Azure Blob Storage and OpenTelemetry/Application Insights
> - Modernize HTTP communication, synchronous API usage, local process/system dependencies, and certificate management for cloud-native, containerized hosting
>
> The migration follows a sequential, dependency-ordered approach: configuration and credential migrations first, followed by communication/IO/performance modernization, then logging modernization, concluding with a security/CVE remediation pass.

---

## Migration Impact Summary

| Application | Original Service | New Azure Service | Authentication | Comments |
|-------------|------------------|-------------------|-----------------|----------|
| TubieTools_Aspire | Local appsettings.json | Azure App Configuration | Managed Identity | SDK-integrated, dynamic refresh |
| TubieTools_Aspire | Plaintext credentials/connection strings | Azure Key Vault | Managed Identity | Hardcoded sensitive data |
| TubieTools_Aspire | Direct HTTP calls | Modernized HTTP communication | N/A | External resource access via HTTP |
| TubieTools_Aspire | Local/network file IO | Modernized file system access | N/A | Local or network IO operations |
| TubieTools_Aspire | Synchronous APIs | Async APIs | N/A | Performance optimization for cloud |
| TubieTools_Aspire | Env-var based connection strings | Modernized connection management | Managed Identity (where applicable) | Connection string security |
| TubieTools_Aspire | Local process start | Modernized system dependencies | N/A | Local system dependencies |
| TubieTools_Aspire | Local certificate store | Azure Key Vault (Certificates) | Managed Identity | Certificate management |
| TubieTools_Aspire | Local file system (static content/paths) | Azure Blob Storage | Managed Identity | Local file / static content |
| TubieTools_Aspire | Local file logging | OpenTelemetry / Application Insights | Managed Identity | Local file logging |
| TubieTools_Aspire | Serilog (file sinks) | OpenTelemetry / Application Insights | Managed Identity | Serilog framework detected |

---

## Open Questions & Questionnaire

- [x] Q: Which Configuration Management solution should be used? → A: App-integrated (SDK, supports dynamic refresh) — Azure App Configuration
- [x] Q: Which Local File solution should be used? → A: Migrate to Azure Blob Storage
