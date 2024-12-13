# Microsoft.Build.Sql.Templates

SQL projects templates for a new project and a sample code analysis rule.

📗 Learn more about SQL projects at https://learn.microsoft.com/sql/tools/sql-database-projects/sql-database-projects

## Installing the templates

The templates are available on [NuGet](https://www.nuget.org/packages/Microsoft.Build.Sql.Templates/).  To install the templates, run the following command:

```bash
dotnet new install Microsoft.Build.Sql.Templates
```

Microsoft.Build.Sql projects and code analysis rules can be created using the `sqlproj` and `sqlcodeanalysis` templates, respectively. Both projects require .NET SDK 8 or higher.

## Using the templates

### SQL project


Creating a new project "AdventureWorks" (`-n` or `--name`):

```bash
dotnet new sqlproj -n "AdventureWorks"
```

Displaying help information for the SQL project template (`-h`):

```bash
dotnet new sqlproj -h
```


Creating a new project "AdventureWorksLT" for Azure SQL Database (`-tp` or `--target-platform`):

```bash
dotnet new sqlproj -n "AdventureWorksLT" -tp "SqlAzureV12"
```

Creating a new project "AdventureWorks" with a `.gitignore` file (-g):

```bash
dotnet new sqlproj -n "AdventureWorks" -g
```

### New sample code analysis rule

Creating a new sample code analysis rule "WaitForDelay" (`-n` or `--name`):

```bash
dotnet new sqlcodeanalysis -n "WaitForDelay"
```

Displaying help information for the SQL code analysis template (`-h`):

```bash
dotnet new sqlcodeanalysis -h
```


## Building the templates

If you want to customize or contribute to the templates, you will need to build and install the templates locally. The following instructions will help you get started.

The templates automatically generate a nupkg on build. To build and install the nupkg locally, run the following command:

```bash
dotnet build
dotnet new install bin/Debug/Microsoft.Build.Sql.Templates.1.0.0.nupkg
```

To uninstall the templates

```bash
dotnet new uninstall Microsoft.Build.Sql.Templates
```


## Resources

- [Primary documentation](https://aka.ms/sqlprojects)
- [CI/CD workflow samples](https://aka.ms/sqlprojects-samples)
- [GitHub repository](https://github.com/microsoft/dacfx)