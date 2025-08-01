// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Build.Sql.Tests
{
    public static class TestUtils
    {
        private const string DotnetToolPathEnvironmentVariable = "DOTNET_TOOL_PATH";
        private const string DotnetRootEnvironmentVariable = "DOTNET_ROOT";
        private const string MSBuildPathEnvironmentVariable = "MSBUILD_EXE_PATH";
        private const string BuildFromSSDTEnvironmentVariable = "BUILD_FROM_SSDT";

        /// <summary>
        /// Returns the full path to the dotnet executable based on the current operating system.
        /// Path to dotnet tool can be set by build pipelines via DotnetToolPathEnvironmentVariable.
        /// </summary>
        public static string DotnetPath
        {
            get
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
        }

#if NETFRAMEWORK
        /// <summary>
        /// Returns the full path to the MSBuild executable based on Visual Studio installation.
        /// </summary>
        public static string MSBuildExePath
        {
            get
            {
                string? msbuildPath = Environment.GetEnvironmentVariable(MSBuildPathEnvironmentVariable);
                if (!string.IsNullOrEmpty(msbuildPath))
                {
                    return msbuildPath;
                }

                // MSBuild may be in PATH already
                msbuildPath = Environment.GetEnvironmentVariable("PATH")
                    ?.Split(Path.PathSeparator)
                    .Select(path => Path.Combine(path, "MSBuild.exe"))
                    .FirstOrDefault(File.Exists);
                if (msbuildPath != null && !string.IsNullOrEmpty(msbuildPath))
                {
                    return msbuildPath;
                }

                // Fallback to default MSBuild path for Visual Studio 2022
                return @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe";
            }
        }

        /// <summary>
        /// Returns true if the environment variable BUILD_FROM_SSDT is set to true.
        /// This controls if we should test build with SSDT binaries or SDK binaries.
        /// </summary>
        public static bool BuildFromSSDT
        {
            get
            {
                string buildFromSSDT = Environment.GetEnvironmentVariable(BuildFromSSDTEnvironmentVariable);
                return bool.TryParse(buildFromSSDT, out bool result) && result;
            }
        }
#endif

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

        /// <summary>
        /// Creates and returns the path to a temporary directory in the current directory.
        /// </summary>
        public static string CreateTempDirectory()
        {
            string tempDir = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
    }
}
