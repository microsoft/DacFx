# Functionality
In SQL projects there are several capabilities that can be specified in the `.sqlproj` file that impact the database model either at project build or deployment.  The following sections describe some of these capabilities that are available for Microsoft.Build.Sql projects.

|Capability|Build time validation|Included in deployment|
|---|---|---|
|[Target platform](#target-platform)|Yes|Optional, yes by default|
|[Database references](#database-references)|Yes|Optional for same database, no by default|
|[Package references](#package-references)|Yes|No|
|[SQLCMD variables](#sqlcmd-variables)|No|Yes|
|[Pre/post-deployment scripts](#prepost-deployment-scripts)|No|Yes|

If you're new to SQL projects, you might also want to check out the [tutorial](Tutorial.md).

## Default SQL files included in build
The SDK specifies a default globbing pattern to include `**/*.sql` from the project root.

* .sql files that do not fall under the default globbing pattern still need to be included explicitly.
* [Pre/post-deployment scripts](#prepost-deployment-scripts) that are specified by the `<PreDeploy>` or `<PostDeploy>` tags are automatically excluded from build.
* To manually exclude a .sql file from the project, add `<Build Remove="filename.sql"/>` to the project file.

## Target platform
The target platform property is contained in the `DSP` tag in the `.sqlproj` file under the `<PropertyGroup>` item.  The target platform is used during project build to validate support for features included in the project and is added to the `.dacpac` file as a property.  By default, during deployment, the target platform is checked against the target database to ensure compatibility.  If the target platform is not supported by the target database, the deployment fails unless an override [publish option](../../tools/sqlpackage/sqlpackage-publish.md) is specified.

```xml
<Project DefaultTargets="Build">
  <Sdk Name="Microsoft.Build.Sql" Version="0.1.9-preview" />
  <PropertyGroup>
    <Name>AdventureWorks</Name>
    <DSP>Microsoft.Data.Tools.Schema.Sql.SqlAzureV12DatabaseSchemaProvider</DSP>
  </PropertyGroup>
...
```

Valid settings for the target platform are:
- `Microsoft.Data.Tools.Schema.Sql.Sql120DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.Sql130DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.Sql140DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.Sql150DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.SqlAzureV12DatabaseSchemaProvider`
- `Microsoft.Data.Tools.Schema.Sql.SqlDwDatabaseSchemaProvider`

## Database references
The database model validation at build time can be extended past the contents of the SQL project through database references. Database references specified in the `.sqlproj` file can reference another SQL project or a `.dacpac` file, representing either another database or more components of the same database.

The following attributes are available for database references that represent another database:
- **DatabaseSqlCmdVariable:** the value is the name of the variable that is used to reference the database
    - Reference setting: `<DatabaseSqlCmdVariable>SomeOtherDatabase</DatabaseSqlCmdVariable>`
    - Usage example: `SELECT * FROM [$(SomeOtherDatabase)].dbo.Table1`
- **ServerSqlCmdVariable:** the value is the name of the variable that is used to reference the server where the database resides. used with DatabaseSqlCmdVariable, when the database is in another server.
    - Reference setting: `<ServerSqlCmdVariable>SomeOtherServer</ServerSqlCmdVariable>`
    - Usage example: `SELECT * FROM [$(SomeOtherServer)].[$(SomeOtherDatabase)].dbo.Table1`
- **DatabaseVariableLiteralValue:** the value is the literal name of the database as used in the SQL project, similar to `DatabaseSqlCmdVariable` but the reference to other database is a literal value
    - Reference setting: `<DatabaseVariableLiteralValue>SomeOtherDatabase</DatabaseVariableLiteralValue>`
    - Usage example: `SELECT * FROM [SomeOtherDatabase].dbo.Table1`

In a SQL project file, a database reference is specified as an `ArtifactReference` item with the `Include` attribute set to the path of the `.dacpac` file.

```xml
...
  <ItemGroup>
    <ArtifactReference Include="SampleA.dacpac">
      <DatabaseSqlCmdVariable>DatabaseA</DatabaseSqlCmdVariable>
    </ArtifactReference>
  </ItemGroup>
</Project>
```

When a database reference is additional objects for the same database as the SQL project, the objects can be included in a [SqlPackage publish](https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-publish) operation by setting the `/p:IncludeCompositeObjects` property to `true`.


## Package references
Package references are used to reference NuGet packages that contain a `.dacpac` file and are used to extend the database model at build time similarly as a [database reference](#database-references).

The following example from a SQL project file references the `Microsoft.SqlServer.Dacpacs` [package](https://www.nuget.org/packages/Microsoft.SqlServer.Dacpacs.Master) for the `master` database.

```xml
...
  <ItemGroup>
    <PackageReference Include="Microsoft.SqlServer.Dacpacs.Master" Version="160.0.0" />
  </ItemGroup>
</Project>
```

In addition to the attributes available for [database references](#database-references), the following `DacpacName` attribute can be specified to select a `.dacpac` from a package that contains multiple `.dacpac` files.

```xml
...
  <ItemGroup>
    <PackageReference Include="Contoso.Applications" Version="160.0.0">
      <DacpacName>Outfitters</DacpacName>
    </PackageReference>
  </ItemGroup>
</Project>
```

## SqlCmd variables
SqlCmd variables can be defined in the `.sqlproj` file and are used to replace tokens in SQL objects and scripts during `.dacpac` [deployment](../../tools/sqlpackage/sqlpackage-publish.md#sqlcmd-variables). The following example from a SQL project file defines a variable named `EnvironmentName` that available for use in the project's objects and scripts.

```xml
...
  <ItemGroup>
    <SqlCmdVariable Include="EnvironmentName">
      <DefaultValue>testing</DefaultValue>
      <Value>$(SqlCmdVar__1)</Value>
    </SqlCmdVariable>
  </ItemGroup>
</Project>
```

```sql
IF '$(EnvironmentName)' = 'testing'
BEING
    -- do something
END
```

When a compiled SQL project (`.dacpac`) is deployed, the value of the variable is replaced with the value specified in the deployment command.  For example, the following command deploys the `AdventureWorks.dacpac` and sets the value of the `EnvironmentName` variable to `production`.

```bash
SqlPackage /Action:Publish /SourceFile:AdventureWorks.dacpac /TargetConnectionString:{connection_string_here} /v:EnvironmentName=production
```

## Pre/post-deployment scripts
Pre- and post-deployment scripts are SQL scripts that are included in the project to be executed during deployment. Pre/post-deployment scrips are included in the `.dacpac` but they are not compiled into or validated with database object model. A pre-deployment script is executed before the database model is applied and a post-deployment script is executed after the database model is applied.  The following example from a SQL project file adds the file `populate-app-settings.sql` as post-deployment script.

```xml
...
  <ItemGroup>
    <PostDeploy Include="populate-app-settings.sql" />
  </ItemGroup>
</Project>
```
