# Microsoft.Build.Sql.Templates


## Using the templates


Creating a new project "AdventureWorks" (`-n` or `--name`):

```bash
dotnet new sqlproj -n "AdventureWorks"
```

Displaying help information for the SQL project template (`-h`):

```bash
dotnet new sqlproj -h
```


Creating a new project "AdventureWorksLT" for Azure SQL Database (`-t` or `--target-platform`):

```bash
dotnet new sqlproj -n "AdventureWorksLT" -t "SqlAzureV12"
```


## Building the templates

If you want to customize or contribute to the templates, you will need to build and install the templates locally. The following instructions will help you get started.

The templates automatically generate a nupkg on build. To build and install the nupkg locally, run the following command:

```bash
dotnet build
dotnet new --install bin/Debug/Microsoft.Build.Sql.Templates.1.0.0.nupkg
```


To uninstall the templates

```bash
dotnet new --uninstall Microsoft.Build.Sql.Templates
```