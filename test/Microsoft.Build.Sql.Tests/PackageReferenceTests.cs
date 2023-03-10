// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Build.Construction;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class PackageReferenceTests : DotnetTestBase
    {
        // The dacpac reference shared by all the tests in this class come from TestData/ReferenceProj.
        // ReferenceProj has only a table Table1 with 2 columns c1 and c2.

        private const string ReferenceProjectName = "ReferenceProj";
        private const string ReferencePackageVersion = "5.5.5";

        /// <summary>
        /// Runs before each test, builds and packs a common reference project into .dacpac file
        /// that will be included as package reference by each test.
        /// </summary>
        [SetUp]
        public void CreateReferencePackage()
        {
            // Copy the reference project folder to working directory
            TestUtils.CopyDirectoryRecursive(Path.Combine(CommonTestDataDirectory, ReferenceProjectName), Path.Combine(WorkingDirectory, ReferenceProjectName));
            
            // Build, pack, and verify output
            string stdOutput, stdError;
            string packagesFolder = Path.Combine(this.WorkingDirectory, "pkg");
            int exitCode = this.RunGenericDotnetCommand($"pack {ReferenceProjectName}/{ReferenceProjectName}.sqlproj -p:Version={ReferencePackageVersion} -o \"{packagesFolder}\"", out stdOutput, out stdError);

            Assert.AreEqual(0, exitCode, "dotnet pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            FileAssert.Exists(Path.Combine(packagesFolder, $"{ReferenceProjectName}.{ReferencePackageVersion}.nupkg"));

            // Delete the reference project folder now that we have a dacpac
            Directory.Delete(Path.Combine(this.WorkingDirectory, ReferenceProjectName), true);
        }

        [Test]
        [Description("Verifies simple package reference scenario")]
        public void VerifySimplePackageReference()
        {
            this.AddPackageReference();

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference is from another database")]
        public void VerifyPackageReferenceDifferentDatabase()
        {
            this.AddPackageReference(databaseSqlcmdVariable: "RefProj");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference uses a database literal instead of SQLCMD variable.")]
        public void VerifyPackageReferenceWithDatabaseVariableLiteral()
        {
            this.AddPackageReference(databaseVariableLiteralValue: "RefProjLit");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference is from another server and another database")]
        public void VerifyPackageReferenceDifferentServerDifferentDatabase()
        {
            this.AddPackageReference(serverSqlcmdVariable: "RefServer", databaseSqlcmdVariable: "RefProj");

            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommandOnProject("build", out stdOutput, out stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        private void AddPackageReference(string serverSqlcmdVariable = "", string databaseSqlcmdVariable = "", string databaseVariableLiteralValue = "", bool? suppressMissingDependenciesErrors = null)
        {
            // Add a package reference to ReferenceProj version 5.5.5
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "PackageReference", new string[] { ReferenceProjectName }, (ProjectItemElement item) => {
                item.AddMetadata("Version", ReferencePackageVersion);

                if (!string.IsNullOrEmpty(serverSqlcmdVariable))
                {
                    item.AddMetadata("ServerSqlCmdVariable", serverSqlcmdVariable);
                }

                if (!string.IsNullOrEmpty(databaseSqlcmdVariable))
                {
                    item.AddMetadata("DatabaseSqlCmdVariable", databaseSqlcmdVariable);
                }

                if (!string.IsNullOrEmpty(databaseVariableLiteralValue))
                {
                    item.AddMetadata("DatabaseVariableLiteralValue", databaseVariableLiteralValue);
                }

                if (suppressMissingDependenciesErrors.HasValue)
                {
                    item.AddMetadata("SuppressMissingDependenciesErrors", suppressMissingDependenciesErrors.ToString());
                }
            });
        }
    }
}