# New SDK-style SQL project with Microsoft.Build.Sql

## Build

To build the project, run the following command:

```bash
dotnet build /p:NetCoreBuild=true
```

🎉 Congrats! You have successfully built the project and now have a `dacpac` to deploy anywhere.

## Publish

To publish the project, the SqlPackage CLI or the SQL Database Projects extension for Azure Data Studio/VS Code is required. The following command will publish the project to a local SQL Server instance:

```bash
./SqlPackage /Action:Publish /SourceFile:bin/Debug/SqlProject1.dacpac /TargetServerName:localhost /TargetDatabaseName:SqlProject1
```

