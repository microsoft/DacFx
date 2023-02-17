# Tutorial
This page walks through creating a new SQL project and publishing it to a database from the command line and any text editor. For more information on using SQL projects in Azure Data Studio or VS Code, check out the [SQL Database Projects extension documentation](https://aka.ms/azuredatastudio-sqlprojects).

## 📁 Create a SQL project
Install the Microsoft.Build.Sql.Templates NuGet package to get started with a new SQL project.

```bash
dotnet new -i Microsoft.Build.Sql.Templates
```

Create a new SQL project using the `sqlproj` [template](src/Microsoft.Build.Sql.Templates/).

```bash
dotnet new sqlproj -n ProductsTutorial
```

## 📝 Add a database object
Add a new table `dbo.Product` in a *.sql* file (`dbo.Product.sql`) alongside the project file.

```sql
CREATE TABLE [dbo].[Product](
    [ProductID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductName] [nvarchar](200) NOT NULL
);
```

## 🛠️ Build the project
Build the project to create a .dacpac file.

```bash
dotnet build
```

## Add more database objects

### Add a stored procedure
We're going to add a stored procedure to the project. Create a new file `dbo.GetProductInfo.sql` alongside the project file.

```sql
CREATE PROCEDURE [dbo].[GetProductInfo]
    @ProductID INT
AS
BEGIN
    SELECT P.ProductID
        , P.ProductName
        , SUM(PO.Quantity) AS TotalQuantity
    FROM dbo.Product P
    LEFT JOIN dbo.ProductOrder PO ON PO.ProductID = P.ProductID
    WHERE P.ProductID = @ProductID
    GROUP BY P.ProductID, P.ProductName
END
```

🚧 We've added a stored procedure that references the `dbo.Product` table we created earlier in addition to a table we haven't created yet (`dbo.ProductOrder`). Running `dotnet build` at this point returns build warnings about unresolved references.

### Adding the missing table
Create a new file `dbo.ProductOrder.sql` alongside the project file for the missing table `dbo.ProductOrder`.

```sql
CREATE TABLE [dbo].[ProductOrder](
    [ProductOrderID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductID] [int] NOT NULL FOREIGN KEY REFERENCES dbo.Product(ProductID),
    [Quantity] [int] NOT NULL
);
```

### Organize the project
We've been creating all our files in the same folder with the `.sqlproj` file. This works, but it's not the best way to organize a project. A common approach is to group objects by schema and/or object type.

Create a new folder `Tables` and move the `dbo.Product.sql` and `dbo.ProductOrder.sql` files into it. Create a new folder `StoredProcedures` and move the `dbo.GetProductInfo.sql` file into it.

When you run `dotnet build` again, the build still succeeds. The build process automatically finds the files in the project folder and subfolders.

## 🛳️ Publish a SQL project

Publish a SQL project to a database using the SqlPackage `publish` command. Learn more about the `publish` command in the [SqlPackage documentation](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-publish), where additional examples and details on the parameters are available.

```bash
# example publish from Azure SQL Database using SQL authentication and a connection string
SqlPackage /Action:Publish /SourceFile:"bin\Debug\ProductsTutorial.dacpac" \
    /TargetConnectionString:"Server=tcp:{yourserver}.database.windows.net,1433;Initial Catalog=ProductsTutorial;User ID=sqladmin;Password={your_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

When the publish command completes, you can connect to the database and view the objects that were created.

### 🧩 (optional) Update the project and publish again
We can update the project by adding a new column to the `dbo.Product` table. Open the existing `dbo.Product.sql` file and add column for `ProductDescription`.

The resulting file is:

```sql
CREATE TABLE [dbo].[Product](
    [ProductID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductName] [nvarchar](200) NOT NULL,
    [ProductDescription] [nvarchar](800) NULL
);
```

When we run `dotnet build` again, the build succeeds and the `ProductsTutorial.dacpac` file is updated.

🪄 We can publish the updated project to the database using the same `publish` command as before.  Running the publish command again will update the existing database schema automatically, there's no need to remove the database first or track the changes manually. 

## (optional) Check the project into source control
One of the benefits of using SQL projects is that the database schema is stored in source control. This makes it easy to collaborate and to deploy changes using CI/CD practices.

When we build the project, the .dacpac file and other artifacts are created in the `bin` and `obj` folders. We can keep these folders out of source control by adding them to the `.gitignore` file or by using the default `.gitignore` for .NET.

The default `.gitignore` for .NET projects can be created by running the following command in the project folder:

```bash
dotnet new gitignore
```

Once you've committed the project to source control, you can push it to a GitHub/remote repository.  You can do this from your IDE or from the command line using the following commands:

**Commit the project to source control**
```bash
git init
git add .
git commit -m "new project"
```

**Push the project to a GitHub repository**
```bash
git remote add origin https://github.com/{yourusername}/{yourrepository}.git
git branch -M main
git push -u origin main
```

## 🚀 (optional) Add a GitHub build/deploy pipeline
If we've pushed our project to a GitHub repository, we can utilize GitHub Actions to build and deploy our project to a database. This is a great way to automate the deployment of database changes.

The pipeline definitions are stored in a `.github/workflows` folder. Create a new file `build-and-deploy.yml` in the `.github/workflows` folder.  We'll use [sql-action](https://github.com/azure/sql-action) to handle building the SQL project and publishing it to a database. It is recommended you check out the [sql-action documentation](https://github.com/azure/sql-action) for more information on how to use it, but an example pipeline definition is shown below to get you started.

*The example pipeline definition below uses a [GitHub secret](https://docs.github.com/en/actions/reference/encrypted-secrets) to store the connection string as `SQL_CONNECTION_STRING`.*

```yaml
# .github/workflows/build-and-deploy.yml
name: Build and deploy SQL project
on: [push]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: azure/sql-action@v2
      with:        
        connection-string: ${{ secrets.SQL_CONNECTION_STRING }}
        path: './ProductsTutorial.sqlproj'
        action: 'publish'
```

## 📚 Learn more
Wow, you made it to the end!  Here are some additional resources to learn more about SQL projects:
- [Microsoft.Build.Sql Functionality More In-Depth](Functionality.md)
- [SQL Database Projects in Azure Data Studio](https://aka.ms/azuredatastudio-sqlprojects)
- [SqlPackage for command line deployments](https://aka.ms/sqlpackage-ref)