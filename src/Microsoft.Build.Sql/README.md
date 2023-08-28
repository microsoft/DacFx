# Microsoft.Build.Sql (Preview)

## Introduction
.NET SDK for database projects (.sqlproj) available in preview.

This SDK is similar to [Microsoft.NET.Sdk](https://learn.microsoft.com/dotnet/core/project-sdk/overview) for .NET projects. It contains all the MSBuild targets and task libraries needed to build a database project into a DACPAC, which can then be used to publish to a database. You can read more about SDK-style projects here: https://learn.microsoft.com/dotnet/core/project-sdk/overview

## Current Status
This project is in preview and we are currently building tests for different functionality. Contributors welcome. Please feel free to open issues for bugs or improvements, or send PRs directly.

The latest release can be found on [Nuget.org](https://www.nuget.org/packages/Microsoft.Build.Sql/).

Building and publishing database project is supported in [Azure Data Studio](https://github.com/microsoft/azuredatastudio) and [VS Code](https://marketplace.visualstudio.com/items?itemName=ms-mssql.sql-database-projects-vscode). Support for SDK-style projects will be added in Visual Studio SQL Server Data Tools (SSDT) in a future release.

## Using this SDK

If you have an existing database project, you can convert it to use this SDK by following the steps in [Converting-Existing.md](docs/Converting-Existing.md).

If you're looking to get started with Microsoft.Build.Sql on a new project, you can follow the brief tutorial in [Tutorial.md](docs/Tutorial.md).

## Telemetry
The .NET SDK for SQL includes a telemetry feature that collects usage data and sends it to Microsoft when you use build commands. The usage data includes exception information when the build command fails. Telemetry data helps the .NET team understand how the tools are used so they can be improved. Information on failures helps the team resolve problems and fix bugs.

### How to opt out
The .NET SDK for SQL telemetry feature is enabled by default for Microsoft distributions of the SDK. To opt out of the telemetry feature, set the DACFX_TELEMETRY_OPTOUT environment variable to 1 or true. Telemetry feature can also be opt out by opting out [.NET SDK telemetry](https://learn.microsoft.com/dotnet/core/tools/telemetry#how-to-opt-out). 

## Documentation
- [Converting Existing Projects](docs/Converting-Existing.md)
- [Functionality](docs/Functionality.md)
- [Troubleshooting](docs/Troubleshooting.md)
- [Tutorial](docs/Tutorial.md)