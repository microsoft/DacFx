# DacFx and Related Components

The Microsoft SQL Server Data-Tier Application Framework (DacFx) is a .NET library which provides application lifecycle services for database development and management for Microsoft SQL Server and Microsoft Azure SQL Databases.

SqlPackage.exe is a command line interface to DacFx and is available for Windows, macOS, and Linux. For more about SqlPackage.exe, check out the [reference page on Microsoft Docs](https://docs.microsoft.com/sql/tools/sqlpackage/sqlpackage).

Microsoft.Build.Sql (preview) is a [MSBuild project SDK](https://docs.microsoft.com/dotnet/core/project-sdk/overview) for SQL projects, compiling T-SQL code to a data-tier application package (dacpac).  The SQL project SDK is available via NuGet ([https://www.nuget.org/packages/Microsoft.Build.Sql/](https://www.nuget.org/packages/Microsoft.Build.Sql/)) and in this [repository](/src/Microsoft.Build.Sql/README.md).

## Repository Focus

### Feedback

This repository is available for transparently triaging and addressing feedback on DacFx, including the NuGet package and the cross-platform CLI SqlPackage.exe. We welcome community interaction and suggestions! For more information on contributing feedback through interacting with issues see [Contributing](CONTRIBUTING.md).

### Related Open Source Projects

This repository is available to make related open source components accessible even from their early stages. Feedback and contributions are welcome!

## Download the Latest Releases

### Microsoft.SqlServer.DacFx on NuGet
[https://www.nuget.org/packages/Microsoft.SqlServer.DacFx](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx)

This NuGet package is a lightweight version of DacFx. Preview versions of the DacFx NuGet are also frequently released. 

### DacFramework.msi and Cross-Platform SqlPackage.exe

If you would like to use the command-line utility SqlPackage.exe for creating and deploying .dacpac and .bacpac packages, you can obtain it by downloading the SqlPackage.exe (.zip file) or DacFramework.msi.

|Platform|Download|Release date|Version|Build
|:---|:---|:---|:---|:---|
|[Windows]|[MSI Installer](https://go.microsoft.com/fwlink/?linkid=2164920)|October 4, 2021| 18.8 | 15.0.5282.3 |
|[macOS .NET Core]|[.zip file](https://go.microsoft.com/fwlink/?linkid=2165009)|October 4, 2021| 18.8| 15.0.5282.3 |
|[Linux .NET Core] |[.zip file](https://go.microsoft.com/fwlink/?linkid=2165008)|October 4, 2021| 18.8| 15.0.5282.3 |
|[Windows .NET Core] |[.zip file](https://go.microsoft.com/fwlink/?linkid=2165007)|October 4, 2021| 18.8| 15.0.5282.3 |


### Microsoft.Build.Sql
[https://www.nuget.org/packages/Microsoft.Build.Sql/](https://www.nuget.org/packages/Microsoft.Build.Sql/)

This NuGet package is a MSBuild SDK for SQL projects.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see [Code of Conduct](CODE_OF_CONDUCT.md).
