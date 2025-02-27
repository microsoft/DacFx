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
            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError, "-bl");

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
    }
}