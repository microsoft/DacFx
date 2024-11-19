# New SQL code analysis rule for SQL projects

## Build

To build the code analysis project, run the following command:

```bash
dotnet build
```

To package the code analysis project as a NuGet package for referencing in a SQL project, run the following command:

```bash
dotnet pack
```

🎉 Congrats! You have successfully built the project and now have a NuGet package to reference in your SQL project.

## Use the code analysis rule in SQL projects

To reference the code analysis project in a SQL project, we need to complete 2 steps:
    1. Publish the code analysis project as a NuGet package.
    1. Reference the code analysis project in the SQL project.

### Publish the code analysis project

We packaged the code analysis project as a NuGet package in the previous step and will publish it to a remote feed or a [local source](https://learn.microsoft.com/dotnet/core/tools/dotnet-nuget-add-source) (folder). Add a folder as a local feed by running the following command:

```bash
dotnet nuget add source c:\packages
```

Copy the NuGet package from `bin/Release` to the local source folder.

### Reference the code analysis project in the SQL project

The following example demonstrates how to reference the code analysis project in a SQL project:

```xml
<ItemGroup>
    <PackageReference Include="Sample.WaitForDelay" Version="1.0.0" />
</ItemGroup>
```

Set either the SQL project property `<RunSqlCodeAnalysis>True</RunSqlCodeAnalysis>` or run `dotnet build /p:RunSqlCodeAnalysis=True` to generate code analysis output in the build log.
