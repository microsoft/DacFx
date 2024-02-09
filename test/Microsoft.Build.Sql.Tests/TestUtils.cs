// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Build.Sql.Tests
{
    public static class TestUtils
    {
        private const string DotnetToolPathEnvironmentVariable = "DOTNET_TOOL_PATH";
        private const string DotnetRootEnvironmentVariable = "DOTNET_ROOT";

        /// <summary>
        /// Returns the full path to the dotnet executable based on the current operating system.
        /// Path to dotnet tool can be set by build pipelines via DotnetToolPathEnvironmentVariable.
        /// </summary>
        public static string GetDotnetPath()
        {
            string dotnetExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
            string? dotnetPath = Environment.GetEnvironmentVariable(DotnetToolPathEnvironmentVariable) ?? Environment.GetEnvironmentVariable(DotnetRootEnvironmentVariable);
            if (string.IsNullOrEmpty(dotnetPath))
            {
                // Determine OS specific dotnet installation path
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    dotnetPath = @"C:\Program Files\dotnet";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    dotnetPath = "/usr/share/dotnet";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    dotnetPath = "/usr/local/share/dotnet";
                }
                else
                {
                    throw new NotSupportedException("Tests are currently not supported on " + RuntimeInformation.OSDescription);
                }
            }

            return Path.Combine(dotnetPath, dotnetExecutable);
        }

        /// <summary>
        /// Copies all files and subdirectories from <paramref name="sourceDirectoryPath"/> to <paramref name="targetDirectoryPath"/>.
        /// </summary>
        public static void CopyDirectoryRecursive(string sourceDirectoryPath, string targetDirectoryPath)
        {
            // Create taret dir if not exists
            Directory.CreateDirectory(targetDirectoryPath);

            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirectoryPath);

            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory not found: " + sourceDirectoryPath);
            }

            // Copy all files
            foreach (var file in sourceDir.EnumerateFiles())
            {
                string destFile = Path.Combine(targetDirectoryPath, file.Name);
                file.CopyTo(destFile, true);
            }

            // Copy all subdirectories recursively
            foreach (var subDir in sourceDir.EnumerateDirectories())
            {
                string destDirName = Path.Combine(targetDirectoryPath, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, destDirName);
            }
        }

        public static string EscapeTestName(string testName)
        {
            return testName.Replace("\"", "_");
        }
    }
}
