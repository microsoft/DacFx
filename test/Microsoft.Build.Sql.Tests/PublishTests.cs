// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Build.Construction;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class PublishTests : DotnetTestBase
    {
        [Test]
        public void VerifySimplePublish()
        {
            int exitCode = this.RunDotnetCommandOnProject("publish", out _, out string stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Publish failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyPublishFolder();
        }

        [Test]
        public void VerifyPublishWithNoBuild()
        {
            // Run build first
            int exitCode = this.RunDotnetCommandOnProject("publish", out _, out string stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Run publish with --no-build
            exitCode = this.RunDotnetCommandOnProject("publish --no-build", out _, out stdError);
            Assert.AreEqual(0, exitCode, "publish failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyPublishFolder();
        }

        [Test]
        public void VerifyPublishkWithIncludedFiles()
        {
            // Add a content file that is copied to output
            string includedContent = Path.Combine(this.WorkingDirectory, "include_content.txt");
            File.WriteAllText(includedContent, "test");
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "Content", new[] { includedContent }, (ProjectItemElement item) =>
            {
                item.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            });

            // Run dotnet publish
            int exitCode = this.RunDotnetCommandOnProject("publish", out _, out string stdError);

            // Verify
            Assert.AreEqual(0, exitCode, "Publish failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyPublishFolder("include_content.txt");
        }

        [Test]
        public void VerifyPublishWithProjectReference()
        {
            // Add a project reference to ReferenceProj, which should be copied to the publish directory
            string tempFolder = TestUtils.CreateTempDirectory();
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "ReferenceProj"), tempFolder);

            this.AddProjectReference(Path.Combine(tempFolder, "ReferenceProj.sqlproj"));

            int exitCode = this.RunDotnetCommandOnProject("publish", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Publish failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyPublishFolder("ReferenceProj.dacpac");
        }

        [Test]
        public void VerifyPublishWithPackageReference()
        {
            // Add a package reference to master.dacpac, which should be copied to the publish directory
            this.AddPackageReference(packageName: "Microsoft.SqlServer.Dacpacs.Azure.Master", version: "160.*");

            int exitCode = this.RunDotnetCommandOnProject("publish", out _, out string stdError);

            Assert.AreEqual(0, exitCode, "Publish failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyPublishFolder("master.dacpac");
        }

        /// <summary>
        /// Verify dacpac is in the publish directory, along with any additional expected files.
        /// </summary>
        private void VerifyPublishFolder(params string[] additionalFiles)
        {
            string publishFolder = Path.Combine(this.GetOutputDirectory(), "publish");
            FileAssert.Exists(Path.Combine(publishFolder, $"{DatabaseProjectName}.dacpac"));
            foreach (string file in additionalFiles) {
                FileAssert.Exists(Path.Combine(publishFolder, file));
            }
        }
    }
}