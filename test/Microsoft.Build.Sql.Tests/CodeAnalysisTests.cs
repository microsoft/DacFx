// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class CodeAnalysisTests : DotnetTestBase
    {
        [Test]
        public void VerifyCodeAnalyzerFromProjectReference()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), TestContext.CurrentContext.Test.Name);
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "CodeAnalyzerSample"), tempFolder);
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "ProjectReference",
                new string[] { Path.Combine(tempFolder, "CodeAnalyzerSample.csproj") },
                item =>
                {
                    item.AddMetadata("PrivateAssets", "All");
                    item.AddMetadata("ReferenceOutputAssembly", "False");
                    item.AddMetadata("OutputItemType", "Analyzer");
                    item.AddMetadata("SetTargetFramework", "TargetFramework=netstandard2.1");
                });

            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "RunSqlCodeAnalysis", "true" },
                { "SqlCodeAnalysisRules", "+!CodeAnalyzerSample.TableNameRule001" }   // Should fail build on this rule
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            Assert.AreNotEqual(0, exitCode, "Build should have failed");
            Assert.IsTrue(stdOutput.Contains("Table name [dbo].[NotAView] ends in View. This can cause confusion and should be avoided"), "Unexpected stderr");
        }
    }
}