# Troubleshooting Microsoft.Build.Sql

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


## Controlling SDK version with global.json
The SDK version can be specified via a [global.json](https://docs.microsoft.com/dotnet/core/tools/global-json?tabs=netcore3x#examples) file:
```json
{
    "msbuild-sdks": {
        "Microsoft.Build.Sql": "0.1.7-preview"
    }
}
```
The `Version` attribute can then be omitted from the `Sdk` element in the project file. This is especially useful if you have multiple database references since all projects must specify the same SDK version for build.

## Other helpful links
- https://learn.microsoft.com/sql/ssdt/alter-dacfx-used-by-ssdt
- https://learn.microsoft.com/sql/tools/sqlpackage/sqlpackage-publish