# Microsoft.Build.Sql

## Introduction
Prototype .NET SDK for database projects (.sqlproj). In preview, use by building the SDK Nuget package locally.

## Current Status
This project is in its early stages and we are currently building tests for different functionality. Contributors welcome, please feel free to share feedback or PRs.

## Known Issues
* Build currently doesn't work with .NET 6. If you have .NET 6 SDK installed, you can workaround this issue by adding a global.json in the same directory as the sqlproj to specify the SDK version.
  Example for using .NET 5 SDK (you can specify any installed SDK version between .NET Core 3.1 and .NET 5):
  ```
  dotnet new globaljson --sdk-version 5.0.400 --force
  ```
  See also: https://github.com/microsoft/DacFx/issues/33

## Using this SDK

### Building this project
1. Build this project by running `dotnet build` from `\src\Microsoft.Build.Sql\`. This will generate a Nuget package inside the `bin` folder.
2. Copy the nupkg file to a well known location, e.g. `C:\local_packages\`

### Adding a local Nuget source
Normally the SDK can be pulled from public Nuget feeds. However since we are still in development mode, this SDK can only be consumed from a local Nuget source.

1. Add a `nuget.config` in the same folder as your sqlproj by running 
   ```
   dotnet new nugetconfig
   ```
2. Edit `nuget.config` by adding a package source pointing to the folder that contains Microsoft.Build.Sql.1.0.0.nupkg
   ```xml
    <packageSources>
      <clear />
      <add key="local" value="{PATH_OF_SDK}" />
    </packageSources>
   ```

### Changes to .sqlproj file
Edit your database project by adding 
```xml
<Sdk Name="Microsoft.Build.Sql" Version="1.0.0" />
``` 
inside `<Project>` tag.
### Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0">
  <Sdk Name="Microsoft.Build.Sql" Version="1.0.0" />
  ...
</Project>
```

## Building .sqlproj 
The database project can be built in SSDT (tested in Visual Studio 2017+) as is. To build the project via dotnet, run:
```
dotnet build /p:NetCoreBuild=true
```
In Visual Studio 2019 (or newer), .NET Core targets can be used by default. Add this property to the sqlproj:
```xml
<NetCoreBuild>True</NetCoreBuild>
```
and the project can be built directly in VS 2019 or `dotnet build` from command line.
