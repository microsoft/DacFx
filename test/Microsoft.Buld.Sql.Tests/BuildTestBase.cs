// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    public abstract class BuildTestBase
    {
        protected const string DatabaseProjectName = "project";

        protected string WorkingDirectory
        {
            get { return Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.Name); }
        }

        protected string TestDataDirectory
        {
            get { return Path.Combine(@"..\..\..\TestData", TestContext.CurrentContext.Test.Name); }
        }

        /// <summary>
        /// Sets up the working directory to test building the project in. The end result will look like:
        /// WorkingDirectory/
        /// ├── pkg/
        /// │   ├── cache/
        /// │   └── Microsoft.Build.Sql.nupkg
        /// ├── nuget.config
        /// ├── project.sqlproj
        /// └── SQL files...
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
            TestUtils.CopyDirectoryRecursive(@"..\..\..\pkg", Path.Combine(this.WorkingDirectory, "pkg"));

            // Copy common project files from Template to WorkingDirectory
            TestUtils.CopyDirectoryRecursive(@"..\..\..\Template", this.WorkingDirectory);

            // Copy test specific files to WorkingDirectory
            TestUtils.CopyDirectoryRecursive(this.TestDataDirectory, this.WorkingDirectory);
        }

        /// <summary>
        /// Calls dotnet build to build the project.
        /// </summary>
        /// <param name="arguments">Any additional arguments to be passed to 'dotnet build'.</param>
        /// <returns>The Exit Code of the dotnet process.</returns>
        /// <remarks>Adapted from Microsoft.VisualStudio.TeamSystem.Data.UnitTests.UTSqlTasks.ExecuteDotNetExe</remarks>
        protected int Build(out string stdOutput, out string stdError, string arguments = "")
        {
            // Append NetCoreBuild to arguments
            arguments += " /p:NetCoreBuild=true";

            // Set up the dotnet process
            ProcessStartInfo dotnetStartInfo = new ProcessStartInfo
            {
                FileName = TestUtils.GetDotnetPath(),
                Arguments = $"build {DatabaseProjectName}.sqlproj {arguments}",
                WorkingDirectory = this.WorkingDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
                // Note: UseShellExecute needs to be false to be able to redirect outputs and run inside workingDirectory.
                // This also requires the full path of the dotnet process be passed to FileName.
            };

            // Setup build output and error handlers
            object threadSharedLock = new object();
            StringBuilder threadShared_ReceivedOutput = new StringBuilder();
            StringBuilder threadShared_ReceivedErrors = new StringBuilder();

            int interlocked_outputCompleted = 0;
            int interlocked_errorsCompleted = 0;

            using Process dotnet = new Process();
            dotnet.StartInfo = dotnetStartInfo;

            // the OutputDataReceived delegateis called on a separate thread as output data arrives from the process
            dotnet.OutputDataReceived += (sender, e) =>
            {

                if (e.Data != null)
                {
                    lock (threadSharedLock)
                    {
                        TestContext.Out.WriteLine(e.Data);
                        threadShared_ReceivedOutput.AppendLine(e.Data);
                    }
                }
                else
                {
                    System.Threading.Interlocked.Increment(ref interlocked_outputCompleted);
                }
            };

            // the ErrorDataReceived delegateis called on a separate thread as output data arrives from the process
            dotnet.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    lock (threadSharedLock)
                    {
                        TestContext.Error.WriteLine(e.Data);
                        threadShared_ReceivedErrors.AppendLine(e.Data);
                    }
                }
                else
                {
                    System.Threading.Interlocked.Increment(ref interlocked_errorsCompleted);
                }
            };

            // Start the build and begin reading the outputs
            TestContext.WriteLine($"Executing {dotnetStartInfo.FileName} {dotnetStartInfo.Arguments} in {dotnetStartInfo.WorkingDirectory}");
            dotnet.Start();
            dotnet.BeginOutputReadLine();
            dotnet.BeginErrorReadLine();

            // Wait for dotnet build to finish with a timeout
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            dotnet.WaitForExit((int)timeout.TotalMilliseconds);

            do
            {
                System.Threading.Thread.MemoryBarrier();
                System.Threading.Thread.Sleep(0);
            }
            while ((interlocked_outputCompleted < 1 || interlocked_errorsCompleted < 1) && timer.Elapsed < timeout);

            lock (threadSharedLock)
            {
                stdOutput = threadShared_ReceivedOutput.ToString();
                stdError = threadShared_ReceivedErrors.ToString();
            }

            return dotnet.ExitCode;
        }

        /// <summary>
        /// Add additional files to be included in build. <paramref name="files"/> paths are relative.
        /// </summary>
        protected void AddBuildFiles(params string[] files)
        {
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "Build", files);
        }

        /// <summary>
        /// Exclude individual files from build. <paramref name="files"/> paths are relative.
        /// </summary>
        protected void RemoveBuildFiles(params string[] files)
        {
            ProjectUtils.AddItemRemoveGroup(this.GetProjectFilePath(), "Build", files);
        }

        /// <summary>
        /// Add pre-deploy scripts to the project. <paramref name="files"/> paths are relative.
        /// </summary>
        protected void AddPreDeployScripts(params string[] files)
        {
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "PreDeploy", files);
        }

        /// <summary>
        /// Add post-deploy scripts to the project. <paramref name="files"/> paths are relative.
        /// </summary>
        protected void AddPostDeployScripts(params string[] files)
        {
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "PostDeploy", files);
        }

        /// <summary>
        /// Returns the full path to the sqlproj file used for this test.
        /// </summary>
        protected string GetProjectFilePath()
        {
            return Path.Combine(this.WorkingDirectory, DatabaseProjectName + ".sqlproj");
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
    }
}