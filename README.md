
# DacFx

The Microsoft SQL Server Data-Tier Application Framework (DacFx) is a component which provides application lifecycle services for database development and management for Microsoft SQL Server and Microsoft Azure SQL Databases.

For more about SqlPackage.exe, check out the [reference page on Microsoft Docs](https://docs.microsoft.com/en-us/sql/tools/sqlpackage?view=sql-server-ver15).

## Feedback Repository

This repository is currently focused on transparently triaging and addressing feedback on DacFx, including the nuget package and the cross platform SqlPackage.exe. We welcome community interaction and suggestions! For more information on contributing feedback through interacting with issues see [Contributing](CONTRIBUTING.md).

## Download the Latest Release


### DacFramework.msi and Cross-Platform SqlPackage.exe
|Platform|Download|Release date|Version|Build
|:---|:---|:---|:---|:---|
|Windows|[MSI Installer](https://go.microsoft.com/fwlink/?linkid=2134206)|June 24, 2020|18.5.1|15.0.4826.1|
|macOS .NET Core |[.zip file](https://go.microsoft.com/fwlink/?linkid=2134312)|June 24, 2020| 18.5.1|15.0.4826.1|
|Linux .NET Core |[.zip file](https://go.microsoft.com/fwlink/?linkid=2134311)|June 24, 2020| 18.5.1|15.0.4826.1|
|Windows .NET Core |[.zip file](https://go.microsoft.com/fwlink/?linkid=2134310)|June 24, 2020| 18.5.1|15.0.4826.1|

### Microsoft.SqlServer.DacFx.x64 on nuget
[https://www.nuget.org/packages/Microsoft.SqlServer.DacFx.x64](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx.x64)

This nuget package is a lightweight version of DacFx. If you would like to use the command-line utility SqlPackage.exe for creating and deploying .dacpac and .bacpac packages, please download DacFramework.msi.


## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see [Code of Conduct](CODE_OF_CONDUCT.md).

