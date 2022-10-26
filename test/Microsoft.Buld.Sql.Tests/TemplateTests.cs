// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NUnit.Framework;
using System.IO;

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
    }
}