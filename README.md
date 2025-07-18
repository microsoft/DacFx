# DacFx and Related Components

|Component|Links|Summary|
|:--|:--|:--|
|SqlPackage|[📦&nbsp;Tool](https://www.nuget.org/packages/Microsoft.SqlPackage)<br/>[📗&nbsp;Docs](https://aka.ms/sqlpackage-ref)|Microsoft.SqlPackage is a cross-platform command-line utility for creating and deploying .dacpac and .bacpac packages. SqlPackage can be installed as a *dotnet tool*.|
|DacFx|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx)<br/>[📘&nbsp;API Docs](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.dac)|The Microsoft SQL Server Data-Tier Application Framework (Microsoft.SqlServer.DacFx) is a .NET library which provides application lifecycle services for database development and management for Microsoft SQL Server and Microsoft Azure SQL Databases. Preview versions of DacFx are frequently released to NuGet.|
|DacpacVerify|[📦&nbsp;Tool](https://www.nuget.org/packages/Microsoft.DacpacVerify)|Microsoft.DacpacVerify is a cross-platform command-line utility for checking that two .dacpac packages match, including pre/post-deployment scripts and SQLCMD variables.|
|Dacpacs.(Master,Msdb)|[📦&nbsp;Master](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Master)<br/>[📦&nbsp;Msdb](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Msdb)|Microsoft.SqlServer.Dacpacs.Master and Microsoft.SqlServer.Dacpacs.Msdb is a set of NuGet packages containing .dacpac files for Microsoft SQL Server system databases (master, msdb) with versions across SQL Server 2008 (100) through SQL Server 2022 (160).|
|Dacpacs.Azure.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Azure.Master)|Microsoft.SqlServer.Dacpacs.Azure.Master is a NuGet package containing a .dacpac file for the Azure SQL Database master database.|
|Dacpacs.DbFabric|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.DbFabric)|Microsoft.SqlServer.Dacpacs.DbFabric is a NuGet package containing a .dacpac file for the SQL database in Fabric system objects.|
|Dacpacs.Synapse.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Synapse.Master)|Microsoft.SqlServer.Dacpacs.Synapse.Master is a NuGet package containing a .dacpac file for the Azure Synapse Analytics master database.|
|Dacpacs.SynapseServerless.Master|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.SynapseServerless.Master)|Microsoft.SqlServer.Dacpacs.SynapseServerless.Master is a NuGet package containing a .dacpac file for the Azure Synapse Analytics serverless SQL pools master database.|
|ScriptDom|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.SqlServer.TransactSql.ScriptDom)<br/>[🛠️&nbsp;Code](https://github.com/microsoft/SqlScriptDOM)<br/>[📘&nbsp;API Docs](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.transactsql.scriptdom)|Microsoft.SqlServer.TransactSql.ScriptDom is a NuGet package containing the Transact-SQL parser [ScriptDOM](https://learn.microsoft.com/dotnet/api/microsoft.sqlserver.transactsql.scriptdom). The [source code](https://github.com/microsoft/SqlScriptDOM) is licensed MIT.|
|Microsoft.Build.Sql|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.Build.Sql)<br/>[🛠️&nbsp;Code](/src/Microsoft.Build.Sql/)|Microsoft.Build.Sql (preview) is a [.NET project SDK](https://docs.microsoft.com/dotnet/core/project-sdk/overview) for SQL projects, compiling T-SQL code to a data-tier application package (dacpac). In preview, [source code](/src/Microsoft.Build.Sql/) in this repository.|
|Project Templates|[📦&nbsp;NuGet](https://www.nuget.org/packages/Microsoft.Build.Sql.Templates)<br/>[🛠️&nbsp;Code](/src/Microsoft.Build.Sql.Templates/)|Microsoft.Build.Sql.Templates (preview) is a set of [.NET project templates](https://learn.microsoft.com/dotnet/core/tools/custom-templates) for SQL projects. In preview, [source code](/src/Microsoft.Build.Sql.Templates/) in this repository.|

## Microsoft.Build.Sql SDK-style projects documentation

- [Overview](https://aka.ms/sqlprojects)
- [Release notes](release-notes/Microsoft.Build.Sql/README.md)
- [Getting Started](https://learn.microsoft.com/sql/tools/sql-database-projects/get-started)
- [Converting Existing Projects](https://learn.microsoft.com/sql/tools/sql-database-projects/howto/convert-original-sql-project)
- [Troubleshooting](https://learn.microsoft.com/sql/tools/sql-database-projects/howto/troubleshoot-sql-project-build)
- [CI/CD Workflow Samples](https://aka.ms/sqlprojects-samples)

## Related tools and libraries

- [GitHub sql-action](https://github.com/azure/sql-action): deploy SQL projects and T-SQL scripts using GitHub Actions
- [Azure DevOps SQL deployments](https://learn.microsoft.com/azure/devops/pipelines/targets/azure-sqldb): deploy SQL projects and run other SqlPackage commands using Azure DevOps
- [Azure Data Studio and VS Code extension for SQL projects](https://aka.ms/azuredatastudio-sqlprojects): create and edit SQL projects in Azure Data Studio and Visual Studio Code on Windows, macOS, and Linux
- [SQL Server Data Tools in Visual Studio](https://learn.microsoft.com/sql/ssdt/): create and edit SQL projects in Visual Studio on Windows

## Quickstart

### 🛠️ Install SqlPackage

SqlPackage is a command line interface to DacFx and is available for Windows, macOS, and Linux. For more about SqlPackage, check out the [reference page on Microsoft Docs](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage).

If you would like to use the command-line utility SqlPackage for creating and deploying .dacpac and .bacpac packages, you can obtain it as a dotnet tool.  The tool is available for Windows, macOS, and Linux.

```bash
dotnet tool install -g microsoft.sqlpackage
```

Alternatively, SqlPackage can be downloaded as a zip file from the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-download).

### 📁 Create a SQL project

Install the Microsoft.Build.Sql.Templates NuGet package to get started with a new SQL project.

```bash
dotnet new install Microsoft.Build.Sql.Templates
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
# example publish to Azure SQL Database using SQL authentication and a connection string
sqlpackage /Action:Publish /SourceFile:"bin/Debug/ProductsTutorial.dacpac" \
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
