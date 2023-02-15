# Tutorial

## 📁 Create a SQL project

Install the Microsoft.Build.Sql.Templates NuGet package to get started with a new SQL project.

```bash
dotnet new -i Microsoft.Build.Sql.Templates
```

Create a new SQL project using the `sqlproj` [template](src/Microsoft.Build.Sql.Templates/).

```bash
dotnet new sqlproj -n AdventureWorks
```

### Add database objects
Add a new table `dbo.Product` in a .SQL file alongside the project file.

```sql
CREATE TABLE [dbo].[Product](
    [ProductID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductName] [nvarchar](200) NOT NULL
);
```

### Build the project

Build the project to create a .dacpac file.

```bash
dotnet build
```

### 🛳️ Publish a SQL project

Publish a SQL project to a database using the SqlPackage `publish` command. Learn more about the `publish` command in the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-publish), where additional examples and details on the parameters are available.

```bash
# example publish from Azure SQL Database using SQL authentication and a connection string
SqlPackage /Action:Publish /SourceFile:"bin\Debug\AdventureWorksLT.dacpac" \
    /TargetConnectionString:"Server=tcp:{yourserver}.database.windows.net,1433;Initial Catalog=AdventureWorksLT;Persist Security Info=False;User ID=sqladmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

