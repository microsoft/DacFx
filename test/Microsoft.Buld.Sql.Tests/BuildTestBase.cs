// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    public abstract class BuildTestBase
    {
        protected const string DatabaseProjectName = "project";

        protected StringBuilder Errors { get; private set; }

        protected string WorkingDirectory
        {
            get { return Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.Name); }
        }

        protected string TestDataDirectory
        {
            get { return Path.Combine(@"..\..\..\TestData", TestContext.CurrentContext.Test.Name); }
        }

        public BuildTestBase()
        {
            this.Errors = new StringBuilder();

            // ClearNugetCache();
        }

        /// <summary>
        /// Sets up the working directory to test building the project in. The end result will look like:
        /// WorkingDirectory/
        /// ├── pkg/
        /// │   ├── cache/
        /// │   └── Microsoft.Build.Sql.nupkg
        /// ├── nuget.config
        /// ├── project.sqlproj
        /// └── *.sql
        /// </summary>
        [SetUp]
        public void EnvironmentSetup()
        {
            // Clear WorkingDirectory from previous test runs if exists
            if (Directory.Exists(this.WorkingDirectory))
            {
                Directory.Delete(this.WorkingDirectory, true);
            }

            // Copy SDK nuget package to Workingdirectory\pkg\
            CopyDirectoryRecursive(@"..\..\..\pkg", Path.Combine(this.WorkingDirectory, "pkg"));

            // Copy common project files from Template to WorkingDirectory
            CopyDirectoryRecursive(@"..\..\..\Template", this.WorkingDirectory);

            // Copy test specific files to WorkingDirectory
            CopyDirectoryRecursive(this.TestDataDirectory, this.WorkingDirectory);
        }

        /// <summary>
        /// Calls dotnet build to build the project.
        /// </summary>
        /// <param name="arguments">Any additional arguments to be passed to 'dotnet build'.</param>
        /// <returns>The Exit Code of the dotnet process.</returns>
        protected int Build(string arguments = "")
        {
            // Append NetCoreBuild to arguments
            arguments += " /p:NetCoreBuild=true";

            // Set up the dotnet process
            ProcessStartInfo dotnetStartInfo = new ProcessStartInfo
            {
                FileName = this.GetDotnetPath(),
                Arguments = $"build {DatabaseProjectName}.sqlproj {arguments}",
                WorkingDirectory = this.WorkingDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
                // Note: UseShellExecute needs to be false to be able to redirect outputs and run inside workingDirectory.
                // This also requires the full path of the dotnet process be passed to FileName.
            };

            TestContext.WriteLine($"Executing {dotnetStartInfo.FileName} {dotnetStartInfo.Arguments} in {dotnetStartInfo.WorkingDirectory}");

            using Process dotnet = new Process();
            dotnet.StartInfo = dotnetStartInfo;

            dotnet.OutputDataReceived += TestOutputHandler;
            dotnet.ErrorDataReceived += ErrorOutputHandler;

            // Start the build and begin reading the outputs
            dotnet.Start();
            dotnet.BeginOutputReadLine();
            dotnet.BeginErrorReadLine();

            // Wait for dotnet build to finish with a timeout
            int timeout = 30 * 1000;    // 30 seconds
            if (dotnet.WaitForExit(timeout))
            {
                return dotnet.ExitCode;
            }
            else
            {
                throw new TimeoutException($"dotnet build timed out after {timeout}ms.");
            }
        }

        /// <summary>
        /// Returns the path to the dacpac file after build is completed.
        /// </summary>
        protected string GetDacpacPath()
        {
            return Path.Combine(this.WorkingDirectory, @"bin\Debug", DatabaseProjectName + ".dacpac");
        }

        protected void VerifyDacPackage(bool expectPreDeployScript = false, bool expectPostDeployScript = false)
        {
            // Verify dacpac exists
            string dacpacPath = this.GetDacpacPath();
            Assert.IsTrue(File.Exists(dacpacPath), "Dacpac not found: " + dacpacPath);

            // Verify pre/post-deploy scripts
            using (DacPackage package = DacPackage.Load(dacpacPath))
            {
                if (expectPreDeployScript)
                {
                    Assert.IsTrue(package.PreDeploymentScript.Length > 0, "PreDeploy script expected but none was found in package.");
                }
                else
                {
                    Assert.IsNull(package.PreDeploymentScript, "PreDeploy script not expected but one was found.");
                }

                if (expectPostDeployScript)
                {
                    Assert.IsTrue(package.PostDeploymentScript.Length > 0, "PostDeploy script expected but none was found in package.");
                }
                else
                {
                    Assert.IsNull(package.PostDeploymentScript, "PostDeploy script not expected but one was found.");
                }
            }
        }

        /// <summary>
        /// Returns the full path to the dotnet executable based on the current operating system.
        /// </summary>
        private string GetDotnetPath()
        {
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

        private void TestOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Data))
            {
                TestContext.Out.WriteLine(e.Data);
            }
        }

        private void ErrorOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e != null && !string.IsNullOrEmpty(e.Data))
            {
                TestContext.Error.WriteLine(e.Data);
                this.Errors.AppendLine(e.Data);
            }
        }

        private void CopyDirectoryRecursive(string sourceDirectoryPath, string targetDirectoryPath)
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