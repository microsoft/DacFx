# Microsoft.Build.Sql (Preview)

## Introduction
.NET SDK for database projects (.sqlproj) available in early preview for use by building the SDK Nuget package locally.

## Current Status
This project is in its early stages and we are currently building tests for different functionality. Contributors welcome, please feel free to share feedback or PRs.

## Using this SDK

### Changes to .sqlproj file
To convert a database project into SDK-style, edit the .sqlproj file by adding
```xml
<Sdk Name="Microsoft.Build.Sql" Version="0.1.1-alpha" />
``` 
inside `<Project>` tag.
### Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0">
  <Sdk Name="Microsoft.Build.Sql" Version="0.1.1-alpha" />
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
