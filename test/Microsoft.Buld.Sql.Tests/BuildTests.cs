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
    public class BuildTests : BuildTestBase
    {
        [Test]
        [Description("Verifies simple build with default settings.")]
        public void SuccessfulSimpleBuild()
        {
            string stdOutput, stdError;
            int exitCode = this.Build(out stdOutput, out stdError);

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
            int exitCode = this.Build(out stdOutput, out stdError);

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
            int exitCode = this.Build(out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage(expectPostDeployScript: true);
        }

        [Test]
        [Description("Verifies build with excluding file from project.")]
        public void BuildWithExclude()
        {
            this.RemoveBuildFiles("Table2.sql");

            string stdOutput, stdError;
            int exitCode = this.Build(out stdOutput, out stdError);

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
            int exitCode = this.Build(out stdOutput, out stdError);

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
            int exitCode = this.Build(out stdOutput, out stdError);

            // Verify failure
            Assert.AreEqual(1, exitCode, "Build is expected to fail.");
        }
    }
}