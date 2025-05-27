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
            // Copy the analyzer project to a temp folder
            string tempFolder = Path.Combine(WorkingDirectory, Path.GetRandomFileName());
            TestUtils.CopyDirectoryRecursive(Path.Combine(this.CommonTestDataDirectory, "CodeAnalyzerSample"), tempFolder);

            // Add the analyzer csproj as a ProjectReference to the test sqlproj
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "ProjectReference",
                new string[] { Path.Combine(tempFolder, "CodeAnalyzerSample.csproj") },
                item =>
                {
                    item.AddMetadata("PrivateAssets", "All");
                    item.AddMetadata("ReferenceOutputAssembly", "False");
                    item.AddMetadata("OutputItemType", "Analyzer");
                    item.AddMetadata("SetTargetFramework", "TargetFramework=netstandard2.1");
                });

            // Set up code analysis properties
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "RunSqlCodeAnalysis", "true" },
                { "SqlCodeAnalysisRules", "+!CodeAnalyzerSample.TableNameRule001" }   // Should fail build on this rule
            });

            int exitCode = this.RunDotnetCommandOnProject("build", out string stdOutput, out string stdError);

            Assert.AreNotEqual(0, exitCode, "Build should have failed");
            Assert.IsTrue(stdOutput.Contains("Table name [dbo].[NotAView] ends in View. This can cause confusion and should be avoided"), "Unexpected stderr");
        }

        [Test]
        public void VerifyCodeAnalyzerFromPackageReference()
        {
            // Set up and create the analyzer package
            string tempFolder = Path.Combine(WorkingDirectory, Path.GetRandomFileName());
            TestUtils.CopyDirectoryRecursive(Path.Combine(CommonTestDataDirectory, "CodeAnalyzerSample"), tempFolder);
            RunGenericDotnetCommand($"pack {Path.Combine(tempFolder, "CodeAnalyzerSample.csproj")} -o {tempFolder} -p:Version=1.1.1-test", out _, out _);
            FileAssert.Exists(Path.Combine(tempFolder, "CodeAnalyzerSample.1.1.1-test.nupkg"), "Analyzer package not found");

            // Add the analyzer package as a PackageReference to the test sqlproj
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "PackageReference",
                new string[] { "CodeAnalyzerSample" },
                item =>
                {
                    item.AddMetadata("Version", "1.1.1-test");
                });

            // Set up code analysis properties
            ProjectUtils.AddProperties(this.GetProjectFilePath(), new Dictionary<string, string>()
            {
                { "RunSqlCodeAnalysis", "true" },
                { "SqlCodeAnalysisRules", "+!CodeAnalyzerSample.TableNameRule001" }   // Should fail build on this rule
            });

            int exitCode = this.RunDotnetCommandOnProject($"build --source {tempFolder}", out string stdOutput, out string stdError);

            Assert.AreNotEqual(0, exitCode, "Build should have failed");
            Assert.IsTrue(stdOutput.Contains("Table name [dbo].[NotAView] ends in View. This can cause confusion and should be avoided"), "Unexpected stderr");
        }
    }
}