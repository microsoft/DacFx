# DacFx and Related Components

|Component|Links|Summary|
|:--|:--|:--|
|Microsoft.Build.Sql|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.Build.Sql)<br/>[🛠️&nbsp;Code](/src/Microsoft.Build.Sql/)|Microsoft.Build.Sql (preview) is a [.NET project SDK](https://docs.microsoft.com/dotnet/core/project-sdk/overview) for SQL projects, compiling T-SQL code to a data-tier application package (dacpac). In preview, [source code](/src/Microsoft.Build.Sql/) in this repository.|
|Project Templates|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.Build.Sql.Templates)<br/>[🛠️&nbsp;Code](/src/Microsoft.Build.Sql.Templates/)|Microsoft.Build.Sql.Templates (preview) is a set of [.NET project templates](https://learn.microsoft.com/dotnet/core/tools/custom-templates) for SQL projects. In preview, [source code](/src/Microsoft.Build.Sql.Templates/) in this repository.|
|SqlPackage|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlPackage)|Microsoft.SqlPackage is a cross-platform command-line utility for creating and deploying .dacpac and .bacpac packages. SqlPackage can be installed as a *dotnet tool*.|
|DacFx|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx)|The Microsoft SQL Server Data-Tier Application Framework (Microsoft.SqlServer.DacFx) is a .NET library which provides application lifecycle services for database development and management for Microsoft SQL Server and Microsoft Azure SQL Databases. Preview versions of DacFx are frequently released to NuGet.|
|Dacpacs.(Master,Msdb)|[📦&nbsp;Master](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Master)<br/>[📦&nbsp;Msdb](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Msdb)|Microsoft.SqlServer.Dacpacs.Master and Microsoft.SqlServer.Dacpacs.Msdb is a set of NuGet packages containing .dacpac files for Microsoft SQL Server system databases (master, msdb) with versions across SQL Server 2008 (100) through SQL Server 2022 (160).|
|Dacpacs.Azure.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Azure.Master)|Microsoft.SqlServer.Dacpacs.Azure.Master is a NuGet package containing a .dacpac file for the Azure SQL Database master database.|
|Dacpacs.Synapse.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Synapse.Master)|Microsoft.SqlServer.Dacpacs.Synapse.Master is a NuGet package containing a .dacpac file for the Azure Synapse Analytics master database.|
|Dacpacs.SynapseServerless.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.SynapseServerless.Master)|Microsoft.SqlServer.Dacpacs.SynapseServerless.Master is a NuGet package containing a .dacpac file for the Azure Synapse Analytics serverless SQL pools master database.|
|ScriptDom|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.TransactSql.ScriptDom)<br/>[🛠️&nbsp;Code](https://github.com/microsoft/SqlScriptDOM)|Microsoft.SqlServer.TransactSql.ScriptDom is a NuGet package containing the Transact-SQL parser [ScriptDOM](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.transactsql.scriptdom). The [source code](https://github.com/microsoft/SqlScriptDOM) is licensed MIT.|

## Microsoft.Build.Sql projects documentation

- [Converting Existing Projects](src/Microsoft.Build.Sql/docs/Converting-Existing.md)
- [Functionality](src/Microsoft.Build.Sql/docs/Functionality.md)
- [Troubleshooting](src/Microsoft.Build.Sql/docs/Troubleshooting.md)
- [Tutorial](src/Microsoft.Build.Sql/docs/Tutorial.md)

## Quickstart

### 🛠️ Install SqlPackage

SqlPackage is a command line interface to DacFx and is available for Windows, macOS, and Linux. For more about SqlPackage, check out the [reference page on Microsoft Docs](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage).

If you would like to use the command-line utility SqlPackage for creating and deploying .dacpac and .bacpac packages, you can obtain it as a dotnet tool.  The tool is available for Windows, macOS, and Linux.

```bash
dotnet tool install -g microsoft.sqlpackage
```

Optionally, SqlPackage can be downloaded as a zip file from the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-download).

### 📁 Create a SQL project

Install the Microsoft.Build.Sql.Templates NuGet package to get started with a new SQL project.

```bash
dotnet new -i Microsoft.Build.Sql.Templates
```

Create a new SQL project using the `sqlproj` [template](src/Microsoft.Build.Sql.Templates/).

```bash
dotnet new sqlproj -n ProductsTutorial
```

Add a new table `dbo.Product` in a *.sql* file alongside the project file.

```sql
CREATE TABLE [dbo].[Product](
    [ProductID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductName] [nvarchar](200) NOT NULL
);
```

Build the project to create a .dacpac file.

```bash
dotnet build
```

### 🛳️ Publish a SQL project

Publish a SQL project to a database using the SqlPackage `publish` command. Learn more about the `publish` command in the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-publish), where additional examples and details on the parameters are available.

```bash
# example publish from Azure SQL Database using SQL authentication and a connection string
SqlPackage /Action:Publish /SourceFile:"bin\Debug\ProductsTutorial.dacpac" \
    /TargetConnectionString:"Server=tcp:{yourserver}.database.windows.net,1433;Initial Catalog=ProductsTutorial;User ID=sqladmin;Password={your_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## Repository Focus

### Feedback

This repository is available for transparently triaging and addressing feedback on DacFx, including the NuGet package and the cross-platform CLI SqlPackage. We welcome community interaction and suggestions! For more information on contributing feedback through interacting with issues see [Contributing](CONTRIBUTING.md).

### Related Open Source Projects

This repository is available to make related open source components accessible even from their early stages. Feedback and contributions are welcome!

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see [Code of Conduct](CODE_OF_CONDUCT.md).
