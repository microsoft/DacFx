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

## Documentation
- [Converting Existing Projects](docs/Converting-Existing.md)
- [Functionality](docs/Functionality.md)
- [Troubleshooting](docs/Troubleshooting.md)
- [Tutorial](docs/Tutorial.md)