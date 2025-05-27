// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
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
            int exitCode = RunDotnetCommandOnProject("pack", out stdOutput, out stdError,
                projectPath: $"{ReferenceProjectName}/{ReferenceProjectName}.sqlproj",
                arguments: $"-p:Version={ReferencePackageVersion} -p:OutputPath=\"{packagesFolder}\"");

            Assert.AreEqual(0, exitCode, "dotnet pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            FileAssert.Exists(Path.Combine(packagesFolder, $"{ReferenceProjectName}.{ReferencePackageVersion}.nupkg"));

            // Delete the reference project folder now that we have a dacpac
            Directory.Delete(Path.Combine(this.WorkingDirectory, ReferenceProjectName), true);

            // Add the reference package directory as a nuget source
            AddLocalNugetSource(packagesFolder, $"ReferenceSource_{TestContext.CurrentContext.Test.Name}", out _, out stdError);
            Assert.AreEqual("", stdError, "Failed to add local nuget source: " + stdError);
        }

        [TearDown]
        public void RemoveReferencePackageSource()
        {
            RemoveLocalNugetSource($"ReferenceSource_{TestContext.CurrentContext.Test.Name}", out _, out string stdError);
            if (!string.IsNullOrEmpty(stdError))
            {
                Assert.Warn("Failed to remove local nuget source: " + stdError);
            }
        }

        [Test]
        [Description("Verifies simple package reference scenario")]
        public void VerifySimplePackageReference()
        {
            this.AddPackageReference(packageName: ReferenceProjectName, version: ReferencePackageVersion);

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference is from another database")]
        public void VerifyPackageReferenceDifferentDatabase()
        {
            this.AddPackageReference(packageName: ReferenceProjectName, version: ReferencePackageVersion, databaseSqlcmdVariable: "RefProj");

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference uses a database literal instead of SQLCMD variable.")]
        public void VerifyPackageReferenceWithDatabaseVariableLiteral()
        {
            this.AddPackageReference(packageName: ReferenceProjectName, version: ReferencePackageVersion, databaseVariableLiteralValue: "RefProjLit");

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where dacpac reference is from another server and another database")]
        public void VerifyPackageReferenceDifferentServerDifferentDatabase()
        {
            this.AddPackageReference(packageName: ReferenceProjectName, version: ReferencePackageVersion, serverSqlcmdVariable: "RefServer", databaseSqlcmdVariable: "RefProj");

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
        }

        [Test]
        [Description("Verifies scenario where build fails to generate script when there is a package reference to master.dacpac (Issue #228)")]
        public void VerifyPackageReferenceToMasterAndGenerateCreateScript()
        {
            // Add PackageReference to master.dacpac and set GenerateCreateScript to true
            this.AddPackageReference(packageName: "Microsoft.SqlServer.Dacpacs.Master", version: "160.*");
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "GenerateCreateScript", "True" }
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            FileAssert.Exists(Path.Combine(this.GetOutputDirectory(), $"{DatabaseProjectName}_Create.sql"));
        }
    }
}