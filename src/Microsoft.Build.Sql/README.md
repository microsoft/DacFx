# Microsoft.Build.Sql (Preview)

## Introduction
.NET SDK for database projects (.sqlproj) available in early preview for use by building the SDK Nuget package locally.

## Current Status
This project is in its early stages and we are currently building tests for different functionality. Contributors welcome, please feel free to share feedback or PRs.

## Using this SDK

### Add Github Packages as Nuget Source
To add Github Packages as a package source, you must first generate a Personal Access Token (PAT) with your Github credentials. [For more information](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#authenticating-to-github-packages)
1. You can generate a PAT from here: https://github.com/settings/tokens
   * Make sure it has *read:packages* scope
2. Save the PAT to a secure place
3. Run this command to add Github Packages as a package source (replace GITHUB_USERNAME and GITHUB_PAT with those of your own):
```
dotnet nuget add source "https://nuget.pkg.github.com/microsoft/index.json" --name github --username "GITHUB_USERNAME" --password "GITHUB_PAT" --store-password-in-clear-text
```

### Changes to .sqlproj file
To convert a database project into SDK-style, edit the .sqlproj file by adding
```xml
<Sdk Name="Microsoft.Build.Sql" Version="0.1.15-alpha" />
``` 
inside `<Project>` tag.
### Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project DefaultTargets="Build" ToolsVersion="4.0">
  <Sdk Name="Microsoft.Build.Sql" Version="0.1.15-alpha" />
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
