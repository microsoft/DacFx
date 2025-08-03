# New SDK-style SQL project with Microsoft.Build.Sql

## Build

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
