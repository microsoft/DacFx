// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public sealed class TemplateTests : DotnetTestBase
    {
        private const string TemplatesPackageId = "Microsoft.Build.Sql.Templates";

        /// <summary>
        /// Installs the template package locally for use by tests
        /// </summary>
        [OneTimeSetUp]
        public void ClassSetup()
        {
            base.EnvironmentSetup();

            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand($"new --install {TemplatesPackageId}", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Template install failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
        }

        /// <summary>
        /// Uninstalls the template package after all tests have run
        /// </summary>
        [OneTimeTearDown]
        public void ClassCleanup()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand($"new --uninstall {TemplatesPackageId}", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Template uninstall failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
        }

        [Test]
        [Description("Verifies installation of the template")]
        public void VerifyTemplateInstallation()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("new --list", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "dotnet new sqlproj failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify our template is in the list of installed templates
            StringAssert.Contains("SQL Server Database Project", stdOutput);
        }

        [Test]
        [Description("Verifies database project template")]
        public void VerifySqlprojTemplate()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("new sqlproj", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "dotnet new sqlproj failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Default project name is the name of the folder
            FileAssert.Exists(Path.Combine(this.WorkingDirectory, "VerifySqlprojTemplate.sqlproj"));
        }

        [Test]
        [Description("Verifies database project template with custom name")]
        public void VerifySqlprojTemplateWithCustomName()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("new sqlproj --name ThisTestProject", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "dotnet new sqlproj failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            FileAssert.Exists(Path.Combine(this.WorkingDirectory, "ThisTestProject", "ThisTestProject.sqlproj"));
        }

        [Test]
        [Description("Verifies database project template with specific target platform")]
        public void VerifySqlprojTemplateWithTargetPlatform()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("new sqlproj --target-platform SqlAzureV12", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "dotnet new sqlproj failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            this.DatabaseProjectName = "VerifySqlprojTemplateWithTargetPlatform";
            this.VerifyTargetPlatform("SqlAzureV12");
        }

        [Test]
        [Description("Verifies the database project template includes a valid project GUID")]
        public void VerifySqlprojTemplateIncludesProjectGuid()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("new sqlproj", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "dotnet new sqlproj failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify the project GUID is present and valid in the generated .sqlproj file
            string projectFilePath = Path.Combine(this.WorkingDirectory, "VerifySqlprojTemplateIncludesProjectGuid.sqlproj");
            FileAssert.Exists(projectFilePath);

            // Parse as XML to properly extract the ProjectGuid element
            var doc = XDocument.Load(projectFilePath);
            var projectGuidElement = doc.Descendants("ProjectGuid").FirstOrDefault();

            Assert.IsNotNull(projectGuidElement, "ProjectGuid element should exist in the .sqlproj file");
            Assert.IsNotEmpty(projectGuidElement!.Value, "ProjectGuid should not be empty");

            // Verify the value is a valid GUID format
            Assert.IsTrue(
                Guid.TryParse(projectGuidElement!.Value, out Guid parsedGuid),
                $"ProjectGuid value '{projectGuidElement!.Value}' is not a valid GUID format");

            Assert.AreNotEqual(Guid.Empty, parsedGuid, "ProjectGuid should not be an empty GUID");
        }
    }
}