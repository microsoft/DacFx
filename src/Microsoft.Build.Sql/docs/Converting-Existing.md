# Converting an existing project to use Microsoft.Build.Sql

This page provides guidance in converting existing SQL projects from Visual Studio SQL Server Data Tools (SSDT) or Azure Data Studio (ADS) to use the Microsoft.Build.Sql SDK.

> [!WARNING]
> Visual Studio does not yet support Microsoft.Build.Sql SDK-style SQL projects. Converting existing projects to use Microsoft.Build.Sql will require you to make further edits through VS Code or Azure Data Studio.


## Changes to .sqlproj file
To convert a database project into SDK-style, edit the .sqlproj file by adding
```xml
<Sdk Name="Microsoft.Build.Sql" Version="0.2.0-preview" />
``` 
inside the `<Project>` tag.

### Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0">
  <Sdk Name="Microsoft.Build.Sql" Version="0.2.0-preview" />
  ...
</Project>
```

### Example statements to remove
```xml
<Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
```
```xml
<Import Condition="..." Project="...\Microsoft.Data.Tools.Schema.SqlTasks.targets"/>
```

* Note: This only applies to the auto-generated statements that come from SSDT or ADS. Please keep any custom `<Import>` targets that you may have added to the project file.

## (Optional) Simplifying the project file
A major advantage of SDK-style projects is a lot of default properties can be defined in the SDK, which means the project file itself can be dramatically simplified.

In addition to the `<Import>` statements that are no longer needed, properties such as build configuration and platform can be removed as well. See the [sample sqlproj file](../../../samples/SdkStyleDatabaseProject/sample.sqlproj) for the minimum properties that are needed to build a DACPAC.

### Default SQL files included in build
The SDK specifies a default globbing pattern to include `**/*.sql` from the project root, so all the explicit `<Build Include="filename.sql"/>` can be removed.
* .sql files that do not fall under the default globbing pattern still need to be included explicitly.
* Pre/post-deployment scripts that are specified by the `<PreDeploy>` or `<PostDeploy>` tags are automatically excluded from build.
* To manually exclude a .sql file from the project, add `<Build Remove="filename.sql"/>` to the project file.
* Even though build will honor the default globbing pattern, .sql files that are not included explicitly will not be displayed inside Solution Explorer in SSDT. Support for this will be added in a future release of SSDT.
