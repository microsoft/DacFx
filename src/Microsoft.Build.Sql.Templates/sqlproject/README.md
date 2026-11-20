# New SDK-style SQL project with Microsoft.Build.Sql

## Build

The project uses `<Project Sdk="Microsoft.Build.Sql">`. Its SDK version is specified in the generated `global.json` under `msbuild-sdks`; update that entry to change the Microsoft.Build.Sql version.

If your repository already has a `global.json`, merge the `msbuild-sdks` entry into that file and remove the generated project-level `global.json`. This keeps the repository's existing .NET SDK settings and lets SQL projects share the same build SDK version.

To build the project, run the following command:

```bash
dotnet build
```

🎉 Congrats! You have successfully built the project and now have a `dacpac` to deploy anywhere.

## Publish

To publish the project, the SqlPackage CLI or the SQL Database Projects extension for VS Code is required. The following command will publish the project to a local SQL Server instance:

```bash
sqlpackage /Action:Publish /SourceFile:bin/Debug/SqlProject1.dacpac /TargetServerName:localhost /TargetDatabaseName:SqlProject1
```

Learn more about authentication and other options for SqlPackage here: https://aka.ms/sqlpackage-ref

### Install SqlPackage CLI

If you would like to use the command-line utility SqlPackage.exe for deploying the `dacpac`, you can obtain it as a dotnet tool.  The tool is available for Windows, macOS, and Linux.

```bash
dotnet tool install -g microsoft.sqlpackage
```
