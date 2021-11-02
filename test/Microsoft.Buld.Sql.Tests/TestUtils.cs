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

        /// <summary>
        /// Returns the full path to the dotnet executable based on the current operating system.
        /// </summary>
        public static string GetDotnetPath()
        {
            // Return dotnet tool path if set by environment variables (pipelines)
            string? dotnetPath = Environment.GetEnvironmentVariable(DotnetToolPathEnvironmentVariable);
            if (!string.IsNullOrEmpty(dotnetPath))
            {
                return dotnetPath;
            }

            // Determine OS specific dotnet installation path
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return @"C:\Program Files\dotnet\dotnet.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "/usr/bin/dotnet/dotnet";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/usr/local/share/dotnet/dotnet";
            }
            else
            {
                throw new NotSupportedException("Tests are currently not supported on " + RuntimeInformation.OSDescription);
            }
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
    }
}