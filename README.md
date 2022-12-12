# DacFx and Related Components

## Packages on NuGet

|Package|Summary|
|:--|:--|
|[Microsoft.SqlServer.DacFx](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx)|The Microsoft SQL Server Data-Tier Application Framework (DacFx) is a .NET library which provides application lifecycle services for database development and management for Microsoft SQL Server and Microsoft Azure SQL Databases. Preview versions of DacFx are frequently released to NuGet.|
|[Microsoft.Build.Sql](https://www.nuget.org/packages/Microsoft.Build.Sql)|Microsoft.Build.Sql (preview) is a [.NET project SDK](https://docs.microsoft.com/dotnet/core/project-sdk/overview) for SQL projects, compiling T-SQL code to a data-tier application package (dacpac). In preview, [source code](/src/Microsoft.Build.Sql/) in this repository.|
|[Microsoft.Build.Sql.Templates](https://www.nuget.org/packages/Microsoft.Build.Sql.Templates)|Microsoft.Build.Sql.Templates (preview) is a set of [.NET project templates](https://learn.microsoft.com/dotnet/core/tools/custom-templates) for SQL projects. In preview, [source code](/src/Microsoft.Build.Sql.Templates/) in this repository.|


## Install SqlPackage

SqlPackage is a command line interface to DacFx and is available for Windows, macOS, and Linux. For more about SqlPackage, check out the [reference page on Microsoft Docs](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage).

If you would like to use the command-line utility SqlPackage for creating and deploying .dacpac and .bacpac packages, you can obtain it as a dotnet tool.  The tool is available for Windows, macOS, and Linux.

```bash
dotnet tool install -g microsoft.sqlpackage
```

Optionally, SqlPackage can be downloaded as a zip file from the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-download).


## Repository Focus

### Feedback

This repository is available for transparently triaging and addressing feedback on DacFx, including the NuGet package and the cross-platform CLI SqlPackage. We welcome community interaction and suggestions! For more information on contributing feedback through interacting with issues see [Contributing](CONTRIBUTING.md).

### Related Open Source Projects

This repository is available to make related open source components accessible even from their early stages. Feedback and contributions are welcome!


## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see [Code of Conduct](CODE_OF_CONDUCT.md).
