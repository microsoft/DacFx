# Microsoft.Build.Sql (Preview)

## Introduction
.NET SDK for database projects (.sqlproj) available in early preview.

This SDK is similar to [Microsoft.NET.Sdk](https://learn.microsoft.com/dotnet/core/project-sdk/overview) for .NET projects. It contains all the MSBuild targets and task libraries needed to build a database project into a DACPAC, which can then be used to publish to a database. You can read more about SDK-style projects here: https://learn.microsoft.com/dotnet/core/project-sdk/overview

## Current Status
This project is in its early stages and we are currently building tests for different functionality. Contributors welcome. Please feel free to open issues for bugs or improvements, or send PRs directly.

The latest release can be found on [Nuget.org](https://www.nuget.org/packages/Microsoft.Build.Sql/).

Building and publishing database project is supported in [Azure Data Studio](https://github.com/microsoft/azuredatastudio). Support for SDK-style projects will be added in SSDT in a future release.

## Using this SDK

### Changes to .sqlproj file
To convert a database project into SDK-style, edit the .sqlproj file by adding
```xml
<Sdk Name="Microsoft.Build.Sql" Version="0.1.3-preview" />
``` 
inside `<Project>` tag.
### Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0">
  <Sdk Name="Microsoft.Build.Sql" Version="0.1.3-preview" />
  ...
</Project>
```

## Building .sqlproj
The database project can be built in SSDT as is. To build the project via dotnet, run:
```
dotnet build /p:NetCoreBuild=true
```

> Note: Building with .NET Core targets is currently unsupported in SSDT. Please make sure the `NetCoreBuild` property is false or not defined when building in SSDT.

## Resolving build errors/warnings
Depending on if your database project originated from SSDT, [Azure Data Studio](https://aka.ms/azuredatastudio-sqlprojects), or VS Code, you may encounter build errors or warnings like these:
```
error MSB4019: The imported project "C:\Program Files\dotnet\sdk\6.0.100-rc.1.21458.32\Microsoft\VisualStudio\v11.0\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets" was not found. Confirm that the expression in the Import declaration "C:\Program Files\dotnet\sdk\6.0.100-rc.1.21458.32\\Microsoft\VisualStudio\v11.0\SSDT\Microsoft.Data.Tools.Schema.SqlTasks.targets" is correct, and that the file exists on disk.
```
or
```
warning MSB4011: "C:\Program Files\dotnet\sdk\6.0.100-rc.1.21458.32\Current\Microsoft.Common.props" cannot be imported again. It was already imported at "C:\test\Database1\Database1.sqlproj (4,3)". This is most likely a build authoring error. This subsequent import will be ignored.
```
To resolve these errors/warnings, remove any default `<Import>` statements from the project file that reference `Microsoft.Data.Tools.Schema.SqlTasks.targets` or `Microsoft.Common.props`.

For system dacpac build errors like:
 ```
C:\Users\user\.nuget\packages\microsoft.build.sql\0.1.3-preview\tools\netstandard2.1\Microsoft.Data.Tools.Schema.SqlTasks.targets(525,5): Build error SQL72027: File "C:\Users\user\.nuget\packages\microsoft.build.sql\0.1.3-preview\tools\netstandard2.1\Extensions\Microsoft\SQLDB\Extensions\SqlServer\150\SqlSchemas\master.dacpac" does not exist.
```
To build with system database references added in VS from the commandline, also pass in `/p:DacPacRootPath="C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE"`, where the path corresponds to the installed version of VS on the machine.

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

In addition to the `<Import>` statements that are no longer needed, properties such as build configuration and platform can be removed as well. See the [sample sqlproj file](../../samples/SdkStyleDatabaseProject/sample.sqlproj) for the minimum properties that are needed to build a DACPAC.

### Default SQL files included in build
The SDK specifies a default globbing pattern to include `**/*.sql` from the project root, so all the explicit `<Build Include="filename.sql"/>` can be removed.
* .sql files that do not fall under the default globbing pattern still need to be included explicitly.
* Pre/post-deployment scripts that are specified by the `<PreDeploy>` or `<PostDeploy>` tags are automatically excluded from build.
* To manually exclude a .sql file from the project, add `<Build Remove="filename.sql"/>` to the project file.
* Even though build will honor the default globbing pattern, .sql files that are not included explicitly will not be displayed inside Solution Explorer in SSDT. Support for this will be added in a future release of SSDT.

### Controlling SDK version with global.json
The SDK version can be specified via a [global.json](https://docs.microsoft.com/dotnet/core/tools/global-json?tabs=netcore3x#examples) file:
```json
{
    "msbuild-sdks": {
        "Microsoft.Build.Sql": "0.1.3-preview"
    }
}
```
The `Version` attribute can then be omitted from the `Sdk` element in the project file. This is especially useful if you have multiple database references since all projects must specify the same SDK version for build.
