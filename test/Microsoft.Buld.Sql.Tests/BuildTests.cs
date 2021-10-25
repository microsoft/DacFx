// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class BuildTests : BuildTestBase
    {
        [Test]
        public void SuccessfulSimpleBuild()
        {
            int exitCode = this.Build();

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + this.Errors.ToString());
            Assert.AreEqual(0, this.Errors.Length);
            this.VerifyDacPackage();
        }

        [Test]
        public void SuccessfulBuildWithPreDeployScript()
        {
            int exitCode = this.Build();

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + this.Errors.ToString());
            Assert.AreEqual(0, this.Errors.Length);
            this.VerifyDacPackage(expectPreDeployScript: true);
        }

        [Test]
        public void SuccessfulBuildWithPostDeployScript()
        {
            int exitCode = this.Build();

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + this.Errors.ToString());
            Assert.AreEqual(0, this.Errors.Length);
            this.VerifyDacPackage(expectPostDeployScript: true);
        }

        [Test]
        public void BuildWithExclude()
        {
            int exitCode = this.Build();

            // Verify success
            Assert.AreEqual(0, exitCode, "Build failed with error " + this.Errors.ToString());
            Assert.AreEqual(0, this.Errors.Length);
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

    }
}