// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class IncrementalBuildTests : DotnetTestBase
    {
        [Test]
        // https://github.com/microsoft/DacFx/issues/448
        public void IncrementalBuildWithNoChanges()
        {
            // Build the project first
            int exitCode = RunDotnetCommandOnProject("build", out _, out string stdError);
            Assert.AreEqual(0, exitCode, "First build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Get the last modified time of the dacpac
            DateTime lastModifiedTime = File.GetLastWriteTime(GetDacpacPath());

            // Run build again and verify it is incremental
            exitCode = RunDotnetCommandOnProject("build", out _, out stdError, arguments: "-flp:v=diag");
            Assert.AreEqual(0, exitCode, "Second build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            StringAssert.Contains(
                "Skipping target \"SqlBuild\" because all output files are up-to-date with respect to the input files.",
                File.ReadAllText(Path.Combine(WorkingDirectory, "msbuild.log")));
            Assert.AreEqual(lastModifiedTime, File.GetLastWriteTime(GetDacpacPath()), "Dacpac should not be modified on incremental build.");
        }

        [Test]
        // https://github.com/microsoft/DacFx/issues/663
        public void VerifyRebuildWithDeletedFile()
        {
            // Build the project first
            int exitCode = RunDotnetCommandOnProject("build", out _, out string stdError);
            Assert.AreEqual(0, exitCode, "First build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify that the dacpac has 2 tables
            using TSqlModel model = new TSqlModel(GetDacpacPath());
            var tables = model.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
            Assert.AreEqual(2, tables.Count(), "Expected 2 tables in the initial build.");

            // Delete Table2.sql from project, build again
            File.Delete(Path.Combine(WorkingDirectory, "Table2.sql"));
            exitCode = RunDotnetCommandOnProject("build", out _, out stdError, arguments: "-bl");
            Assert.AreEqual(0, exitCode, "Second build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);

            // Verify that the dacpac now has only 1 table
            using TSqlModel updatedModel = new TSqlModel(GetDacpacPath());
            var updatedTables = updatedModel.GetObjects(DacQueryScopes.UserDefined, ModelSchema.Table);
            Assert.AreEqual(1, updatedTables.Count(), "Expected 1 table after removing one table from the project.");
        }
    }
}