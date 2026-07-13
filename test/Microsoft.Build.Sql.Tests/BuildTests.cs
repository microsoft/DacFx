// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class BuildTests : DotnetTestBase
    {
        [Test]
        [Description("Verifies simple build with default settings.")]
        public void SuccessfulSimpleBuild()
        {
            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies build with pre-deployment script.")]
        public void SuccessfulBuildWithPreDeployScript()
        {
            this.AddPreDeployScripts("Script.PreDeployment1.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage(expectPreDeployScript: true);
        }

        [Test]
        [Description("Verifies build with post-deployment script.")]
        public void SuccessfulBuildWithPostDeployScript()
        {
            this.AddPostDeployScripts("Script.PostDeployment1.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage(expectPostDeployScript: true);
        }

        [Test]
        [Description("Verifies build with deployment extension configuration script.")]
        public void SuccessfulBuildWithDeploymentExtensionConfigurationScript()
        {
            this.RemoveBuildFiles("Table2.sql"); 
            this.AddDeploymentExtensionConfigurationScripts("Table2.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify the Table2 is not part of the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                Assert.IsTrue(tables.Any(), "Expected at least 1 table in the model.");
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Fail("Table2 should have been excluded from the model.");
                    }
                }
            }
        }

        [Test]
        [Description("Verifies build with build extension configuration script.")]
        public void SuccessfulBuildWithBuildExtensionConfigurationScript()
        {
            this.RemoveBuildFiles("Table2.sql"); 
            this.AddBuildExtensionConfigurationScripts("Table2.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify the Table2 is not part of the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                Assert.IsTrue(tables.Any(), "Expected at least 1 table in the model.");
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Fail("Table2 should have been excluded from the model.");
                    }
                }
            }
        }

        [Test]
        [Description("Verifies build with excluding file from project.")]
        public void BuildWithExclude()
        {
            this.RemoveBuildFiles("Table2.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify the excluded Table2 is not part of the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                Assert.IsTrue(tables.Any(), "Expected at least 1 table in the model.");
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Fail("Table2 should have been excluded from the model.");
                    }
                }
            }
        }

        [Test]
        [Description("Verifies build with including file from outside project folder.")]
        public void BuildWithIncludeExternalFile()
        {
            // Generate a new file
            string tempFile = Path.Combine(Path.GetTempPath(), "test.sql");
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            File.WriteAllText(tempFile, "CREATE TABLE [dbo].[Table2] ( C1 INT NOT NULL )");

            // Include the file in project build
            this.AddBuildFiles(tempFile);

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify the Table2 is included in the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                bool found = false;
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(found, "Table2 is supposed to be included in model but not found.");
                File.Delete(tempFile);
            }
        }

        [Test]
        [Description("Verifies build fails when a script references an object not defined by other scripts in the project.")]
        public void VerifyBuildFailureWithUnresolvedReference()
        {
            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify failure
            Assert.AreEqual(1, exitCode, "Build is expected to fail.");
        }

        [Test]
        [Description("Verifies build with a project reference.")]
        public void VerifyBuildWithProjectReference()
        {
            // We will copy the ReferenceProj to a temp folder and then add it as project reference
            string tempFolder = TestUtils.CreateTempDirectory();
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "ReferenceProj"), tempFolder);

            this.AddProjectReference(Path.Combine(tempFolder, "ReferenceProj.sqlproj"));

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            Directory.Delete(tempFolder, true);
        }

        [Test]
        [Description("Verifies build with a project reference where the referenced project is inside the current folder.")]
        public void VerifyBuildWithProjectReferenceInSubdirectory()
        {
            this.AddProjectReference("ReferenceProj/ReferenceProj.sqlproj");

            // Since reference proj is in a subdirectory it gets picked up by default globbing pattern, excluding it here
            this.RemoveBuildFiles("ReferenceProj/**/*.*");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Issue #88: Verifies build with a .sql file that is included as None should not be part of schema.")]
        public void VerifyBuildWithNoneIncludeSqlFile()
        {
            // Table2.sql is included for build by default, specifying it as <None Include...> should remove it from build.
            this.AddNoneScripts("Table2.sql");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify the excluded Table2 is not part of the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                Assert.IsTrue(tables.Any(), "Expected at least 1 table in the model.");
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Fail("Table2 should have been excluded from the model.");
                    }
                }
            }
        }

        [Test]
        [Description("Issue #316: Verifies build with central package management turned on.")]
        public void VerifyBuildWithCentralPackageManagement()
        {
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "ManagePackageVersionsCentrally", "True" }
            });

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/278
        public void VerifyBuildWithTransitiveProjectReferences()
        {
            string projectA = Path.Combine(WorkingDirectory, "A", "A.sqlproj");
            string projectB = Path.Combine(WorkingDirectory, "B", "B.sqlproj");

            // Add A.sqlproj as a reference in B.sqlproj
            ProjectUtils.AddItemGroup(projectB, "ProjectReference", new string[] { projectA });

            // Add B.sqlproj as a reference in the main project
            this.AddProjectReference(projectB);

            // Build and verify a.dacpac is copied to the output directory
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            FileAssert.Exists(Path.Combine(this.GetOutputDirectory(), "A.dacpac"));
            FileAssert.Exists(Path.Combine(this.GetOutputDirectory(), "B.dacpac"));
        }

        [Test]
        [TestCase("net46")]
        [TestCase("net461")]
        [TestCase("net462")]
        [TestCase("net47")]
        [TestCase("net471")]
        [TestCase("net472")]
        [TestCase("net48")]
        [TestCase("net481")]
        [TestCase("netstandard2.1")]
        [TestCase("netcoreapp3.1")]
        [TestCase("net5.0")]
        [TestCase("net6.0")]
        [TestCase("net7.0")]
        [TestCase("net8.0")]
#if NET9_0_OR_GREATER
        [TestCase("net9.0")]
#endif
#if NET10_0_OR_GREATER
        [TestCase("net10.0")]
#endif
        // https://github.com/microsoft/DacFx/issues/330
        public void VerifyBuildWithDifferentTargetFrameworks(string targetFramework)
        {
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "TargetFramework", targetFramework }
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies that default TargetFramework is netstandard2.1 when no TargetFramework is specified.")]
        public void VerifyDefaultTargetFrameworkIsNetStandard21()
        {
            // Add a target to print the resolved TargetFramework property
            ProjectUtils.AddTarget(GetProjectFilePath(), "PrintTargetFrameworkInfo", target =>
            {
                target.AfterTargets = "Build";
                var messageTask = target.AddTask("Message");
                messageTask.SetParameter("Text", "ResolvedTargetFramework=$(TargetFramework)");
                messageTask.SetParameter("Importance", "high");
            });

            // Build without explicitly setting TargetFramework - should use default netstandard2.1
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify default TargetFramework is netstandard2.1
            StringAssert.Contains("ResolvedTargetFramework=netstandard2.1", stdOutput,
                "Default TargetFramework should be netstandard2.1 when not explicitly set.");
        }

        [Test]
        [Description("Verifies that TargetFrameworkVersion and TargetFrameworkMoniker are correctly derived when TargetFramework is overridden to net472.")]
        public void VerifyTargetFrameworkVersionCorrectWhenOverriddenToNet472()
        {
            // Override TargetFramework to net472
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "TargetFramework", "net472" }
            });

            // Add a target to print the resolved TF/TFV/TFM properties
            ProjectUtils.AddTarget(GetProjectFilePath(), "PrintTargetFrameworkInfo", target =>
            {
                target.AfterTargets = "Build";
                var messageTask = target.AddTask("Message");
                messageTask.SetParameter("Text", "ResolvedTF=$(TargetFramework)|ResolvedTFV=$(TargetFrameworkVersion)|ResolvedTFM=$(TargetFrameworkMoniker)");
                messageTask.SetParameter("Importance", "high");
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify TFV and TFM are correctly derived for net472 (not stale netstandard2.1 values)
            StringAssert.Contains("ResolvedTF=net472", stdOutput,
                "TargetFramework should be net472.");
            StringAssert.Contains("ResolvedTFV=v4.7.2", stdOutput,
                "TargetFrameworkVersion should be v4.7.2, not the stale v2.1 from Sdk.props default.");
            StringAssert.Contains("ResolvedTFM=.NETFramework,Version=v4.7.2", stdOutput,
                "TargetFrameworkMoniker should be .NETFramework,Version=v4.7.2, not the stale .NETStandard value.");
        }

        [Test]
        [Description("Verifies that TargetFrameworkVersion and TargetFrameworkMoniker match when TargetFramework stays at the default netstandard2.1.")]
        public void VerifyTargetFrameworkVersionCorrectWithDefaultNetStandard21()
        {
            // Add a target to print the resolved TF/TFV/TFM properties (no explicit TargetFramework set)
            ProjectUtils.AddTarget(GetProjectFilePath(), "PrintTargetFrameworkInfo", target =>
            {
                target.AfterTargets = "Build";
                var messageTask = target.AddTask("Message");
                messageTask.SetParameter("Text", "ResolvedTF=$(TargetFramework)|ResolvedTFV=$(TargetFrameworkVersion)|ResolvedTFM=$(TargetFrameworkMoniker)");
                messageTask.SetParameter("Importance", "high");
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify TFV and TFM are consistent with netstandard2.1
            StringAssert.Contains("ResolvedTF=netstandard2.1", stdOutput,
                "TargetFramework should be netstandard2.1.");
            StringAssert.Contains("ResolvedTFV=v2.1", stdOutput,
                "TargetFrameworkVersion should be v2.1 for netstandard2.1.");
            StringAssert.Contains("ResolvedTFM=.NETStandard,Version=v2.1", stdOutput,
                "TargetFrameworkMoniker should be .NETStandard,Version=v2.1.");
        }

        [Test]
        [Description("Verifies that TargetFrameworkVersion is correctly derived when TargetFramework is overridden to net8.0.")]
        public void VerifyTargetFrameworkVersionCorrectWhenOverriddenToNet80()
        {
            // Override TargetFramework to net8.0
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "TargetFramework", "net8.0" }
            });

            // Add a target to print the resolved TF/TFV/TFM properties
            ProjectUtils.AddTarget(GetProjectFilePath(), "PrintTargetFrameworkInfo", target =>
            {
                target.AfterTargets = "Build";
                var messageTask = target.AddTask("Message");
                messageTask.SetParameter("Text", "ResolvedTF=$(TargetFramework)|ResolvedTFV=$(TargetFrameworkVersion)|ResolvedTFM=$(TargetFrameworkMoniker)");
                messageTask.SetParameter("Importance", "high");
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Verify TFV and TFM are correctly derived for net8.0 (not stale netstandard2.1 values)
            StringAssert.Contains("ResolvedTF=net8.0", stdOutput,
                "TargetFramework should be net8.0.");
            StringAssert.Contains("ResolvedTFV=v8.0", stdOutput,
                "TargetFrameworkVersion should be v8.0, not the stale v2.1 from Sdk.props default.");
            StringAssert.Contains("ResolvedTFM=.NETCoreApp,Version=v8.0", stdOutput,
                "TargetFrameworkMoniker should be .NETCoreApp,Version=v8.0, not the stale .NETStandard value.");
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/117
        public void VerifyBuildWithReleaseConfiguration()
        {
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "Configuration", "Release" }
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            FileAssert.Exists(Path.Combine(WorkingDirectory, "bin", "Release", DatabaseProjectName + ".dacpac"));
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/103
        public void VerifyBuildWithIncludeFiles()
        {
            // Post-deployment script includes Table2.sql which creates Table2, it should not be part of the model
            this.AddPostDeployScripts("Script.PostDeployment1.sql");
            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage(expectPostDeployScript: true);

            // Verify the Table2 is not part of the model
            using (TSqlModel model = new TSqlModel(this.GetDacpacPath()))
            {
                var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
                Assert.IsTrue(tables.Any(), "Expected at least 1 table in the model.");
                foreach (var table in tables)
                {
                    if (table.Name.ToString().IndexOf("Table2", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Fail("Table2 should have been excluded from the model.");
                    }
                }
            }
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/520
        public void BuildWithExternalReference()
        {
            // Build a ReferenceProj with a table
            string tempFolder = TestUtils.CreateTempDirectory();
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "ReferenceProj"), tempFolder);

            // Add project reference and build with a synonym created from the external table, and a view on the synonym
            this.AddProjectReference(Path.Combine(tempFolder, "ReferenceProj.sqlproj"), databaseSqlcmdVariable: "ReferenceDb");
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            Directory.Delete(tempFolder, true);
        }

        [Test]
        [Description("Verifies that a sqlproj referencing a prebuilt .dacpac directly via an ArtifactReference (database reference) builds and resolves the reference. This exercises the ArtifactReference code path (as opposed to ProjectReference or PackageReference) and confirms _RemoveDacpacFromCompileReferences does not strip it from the SQL model. See https://github.com/microsoft/DacFx/issues/785")]
        public void BuildWithDacpacArtifactReference()
        {
            // Build a ReferenceProj to produce a .dacpac that we can reference directly.
            string tempFolder = TestUtils.CreateTempDirectory();
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "ReferenceProj"), tempFolder);

            // The temp folder lives under the repo tree, so copy the isolation files from the test Template
            // to stop it inheriting the repo's Directory.Build.props (artifacts output layout / central package
            // management). This keeps the dacpac at the standard bin/Debug path.
            foreach (string isolationFile in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props" })
            {
                File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, "Template", isolationFile), Path.Combine(tempFolder, isolationFile), overwrite: true);
            }

            string referenceProjectPath = Path.Combine(tempFolder, "ReferenceProj.sqlproj");
            int referenceExitCode = this.RunDotnetCommandOnProject("build", out _, out string referenceStdError, projectPath: referenceProjectPath);
            Assert.AreEqual(0, referenceExitCode, "Reference project build failed with error " + referenceStdError);

            string dacpacPath = Path.Combine(tempFolder, "bin", "Debug", "ReferenceProj.dacpac");
            FileAssert.Exists(dacpacPath, "Reference project dacpac was not produced.");

            // Reference the prebuilt dacpac directly as an ArtifactReference and consume it via a synonym + view
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "ArtifactReference", new string[] { dacpacPath }, item =>
            {
                item.AddMetadata("DatabaseSqlCmdVariable", "ReferenceDb");
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            Directory.Delete(tempFolder, true);
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/561
        public void FailBuildOnDuplicatedItems()
        {
            // Add a file that should be included in the default globbing pattern already
            this.AddBuildFiles("Table1.sql");

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out _);

            // Verify failure
            Assert.AreEqual(1, exitCode, "Build is expected to fail.");
        }

        [Test]
        public void BuildWithDefaultItemsDisabled()
        {
            // Add a file that should be included in the default globbing pattern already
            this.AddBuildFiles("Table1.sql");

            // Disable default items
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "EnableDefaultSqlItems", "False" }
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/446
        public void BuildWithArtifactsOutput()
        {
            // Set UseArtifactsOutput to true in Directory.Build.props
            File.WriteAllText(Path.Combine(WorkingDirectory, "Directory.Build.props"), @"
<Project>
    <PropertyGroup>
        <UseArtifactsOutput>true</UseArtifactsOutput>
    </PropertyGroup>
</Project>");

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            FileAssert.Exists(Path.Combine(WorkingDirectory, "artifacts", "bin", DatabaseProjectName, "debug", DatabaseProjectName + ".dacpac"));
            FileAssert.DoesNotExist(GetDacpacPath());
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/450
        public void VerifyBuildTargetPath()
        {
            // Add a target to print the target path
            ProjectUtils.AddTarget(GetProjectFilePath(), "PrintTargetPath", target =>
            {
                target.AfterTargets = "AfterBuild";
                var messageTask = target.AddTask("Message");
                messageTask.SetParameter("Text", "Target path from PrintTargetPath: $(TargetPath)");
                messageTask.SetParameter("Importance", "high");
            });

            // Build the project and verify the target path is printed
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            StringAssert.Contains($"Target path from PrintTargetPath: {GetDacpacPath()}", stdOutput, "Target path not found in output.");
        }

        [Test]
        public void VersionCheckTest()
        {
            // Skip this test if running on Azure DevOps
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_JOBNAME")))
            {
                Assert.Ignore("Skipping version check test on Azure DevOps.");
            }

            // Since our test version is 1.x, we should get a warning about a newer version being available
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            StringAssert.Contains("A newer version of Microsoft.Build.Sql is available", stdOutput, "Version check warning not found in output.");
            this.VerifyDacPackage();
            FileAssert.Exists(NuGetClient.GetVersionCacheFilePath("Microsoft.Build.Sql"), "Version cache file should exist after fetching the version.");
        }

#if NETFRAMEWORK
        [Test]
        [Description("Verifies that a SQLCLR project (C# source compiled into a Framework CLR assembly loadable by SQL Server's in-server CLR host) builds under full-framework MSBuild and the assembly is included in the dacpac. See https://github.com/microsoft/DacFx/issues/785")]
        public void SuccessfulSqlClrBuild()
        {
            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            using TSqlModel model = new TSqlModel(this.GetDacpacPath());
            var assemblies = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Assembly).ToList();
            Assert.IsTrue(assemblies.Any(), "Expected at least one SQL CLR assembly in the dacpac, but none was found.");
        }
#endif

#if !NETFRAMEWORK
        [Test]
        [Description("Verifies that a SQL project can be added to a .sln solution and built via dotnet sln add.")]
        public void VerifySolutionAddAndBuild()
        {
            // Create a new solution
            int exitCode = this.RunGenericDotnetCommand("new sln -n SDKTestSolution --format sln", out string stdOutput, out string stdError);
            Assert.AreEqual(0, exitCode, "Solution creation failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Add the existing test project to the solution
            string projectPath = Path.GetFileName(this.GetProjectFilePath());
            exitCode = this.RunGenericDotnetCommand($"sln SDKTestSolution.sln add {projectPath}", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Adding project to solution failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            StringAssert.Contains("added to the solution", stdOutput, "Expected success message not found.");

            // Verify the solution file contains our project
            string slnContent = File.ReadAllText(Path.Combine(this.WorkingDirectory, "SDKTestSolution.sln"));
            StringAssert.Contains(".sqlproj", slnContent, "Project not found in solution file.");

            // Build the solution
            exitCode = this.RunGenericDotnetCommand("build SDKTestSolution.sln --verbosity normal", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Solution build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify dacpac was created
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies that a SQL project can be added to a .slnx solution and built via dotnet sln add.")]
        public void VerifySolutionAddAndBuildSlnx()
        {
            // Create a new solution using slnx format
            int exitCode = this.RunGenericDotnetCommand("new sln -n SDKTestSolution --format slnx", out string stdOutput, out string stdError);
            Assert.AreEqual(0, exitCode, "Solution creation failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Add the existing test project to the solution
            string projectPath = Path.GetFileName(this.GetProjectFilePath());
            exitCode = this.RunGenericDotnetCommand($"sln SDKTestSolution.slnx add {projectPath}", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Adding project to solution failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            StringAssert.Contains("added to the solution", stdOutput, "Expected success message not found.");

            // Verify the solution file contains our project
            string slnxContent = File.ReadAllText(Path.Combine(this.WorkingDirectory, "SDKTestSolution.slnx"));
            StringAssert.Contains(".sqlproj", slnxContent, "Project not found in solution file.");

            // Build the solution
            exitCode = this.RunGenericDotnetCommand("build SDKTestSolution.slnx --verbosity normal", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Solution build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify dacpac was created
            this.VerifyDacPackage();
        }
#endif
    }
}