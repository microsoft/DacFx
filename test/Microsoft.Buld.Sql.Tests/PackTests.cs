// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using NuGet.Packaging;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class PackTests : DotnetTestBase
    {
        [Test]
        [Description("Verifies pack with default settings.")]
        public void VerifySimplePack()
        {
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("pack", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyNugetPackage();
        }

        [Test]
        [Description("Verifies packing separately after build.")]
        public void VerifyPackWithNoBuild()
        {
            // Run build first
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("build", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Run pack with --no-build
            exitCode = this.RunGenericDotnetCommand("pack --no-build", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyNugetPackage();
        }

        [Test]
        [Description("Verifies pack with custom defined properties.")]
        public void VerifyPackWithCustomProperties()
        {
            // Populate project file with custom properties: https://learn.microsoft.com/nuget/reference/msbuild-targets#pack-target
            string customPackageId = "MyCustomPackageId";
            string customPackageVersion = "3.4.5-test";
            var properties = new List<(string NuspecAttributeName, string MSBuildPropertyName, string Value)>()
            {
                ("Id", "PackageId", customPackageId),
                ("Version", "PackageVersion", customPackageVersion),
                ("Authors", "Authors", "MyCustomAuthors"),
                ("Title", "Title", "MyCustomTitle"),
                ("Description", "Description", "My custom description"),
                ("Copyright", "Copyright", "My custom copyright"),
                ("RequireLicenseAcceptance", "PackageRequireLicenseAcceptance", "true"),
                ("license", "PackageLicenseExpression", "MIT"),
                ("PackageType", "PackageType", "MyCustomPackageType")   // This one is a collection
            };

            ProjectUtils.AddProperties(this.GetProjectFilePath(), 
                properties.Select(p => new KeyValuePair<string, string>(p.MSBuildPropertyName, p.Value)));

            // Pack
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("pack", out stdOutput, out stdError);

            // Verify
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyNugetPackage($"{customPackageId}.{customPackageVersion}.nupkg", (PackageArchiveReader package) => {
                // Verify every custom property is set correctly in the nuspec
                foreach (var property in properties)
                {
                    if (property.NuspecAttributeName.Equals("PackageType"))
                    {
                        // PackageType is a collection, so we verify our custom type is one of the items
                        Assert.IsTrue(package.GetPackageTypes().Any(t => t.Name.Equals(property.Value, StringComparison.OrdinalIgnoreCase)),
                            $"Expected '{property.Value}' to be one of the package types.");
                    }
                    else
                    {
                        Assert.AreEqual(property.Value, package.NuspecReader.GetMetadataValue(property.NuspecAttributeName),
                            $"Difference found in nuspec attribute '{property.NuspecAttributeName}'");
                    }
                }
            });
        }

        [Test]
        [Description("Verifies pack with including content files.")]
        public void VerifyPackWithIncludedFiles()
        {
            // Add a content file, which is packed by default: https://learn.microsoft.com/nuget/reference/msbuild-targets#including-content-in-a-package
            string includedContent = Path.Combine(this.WorkingDirectory, "include_content.txt");
            File.WriteAllText(includedContent, "test");
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "Content", new[] { includedContent });

            // Add a content file with Pack = false
            string excludedContent = Path.Combine(this.WorkingDirectory, "exclude_content.txt");
            File.WriteAllText(excludedContent, "test");
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "Content", new[] { excludedContent }, (ProjectItemElement item) =>
            {
                item.AddMetadata("Pack", "false");
            });

            // Add a none file and pack it to the tools folder
            string includedNone = Path.Combine(this.WorkingDirectory, "none.txt");
            File.WriteAllText(includedNone, "test");
            ProjectUtils.AddItemGroup(this.GetProjectFilePath(), "None", new[] { includedNone }, (ProjectItemElement item) =>
            {
                item.AddMetadata("Pack", "true");
                item.AddMetadata("PackagePath", "tools/");
            });

            // Run dotnet pack
            string stdOutput, stdError;
            int exitCode = this.RunGenericDotnetCommand("pack", out stdOutput, out stdError);

            // Verify
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyNugetPackage(test: (PackageArchiveReader package) =>
            {
                var files = package.GetFiles();
                Assert.IsTrue(files.Any(f => f.Equals("content/include_content.txt", StringComparison.OrdinalIgnoreCase)),
                    "Expected content/include_content.txt to be in the packaged file list.");
                Assert.IsFalse(files.Any(f => f.Contains("exclude_content.txt", StringComparison.OrdinalIgnoreCase)),
                    "Expected exclude_content.txt to be excluded from the packaged file list.");
                Assert.IsTrue(files.Any(f => f.Equals("tools/none.txt", StringComparison.OrdinalIgnoreCase)),
                    "Expected tools/none.txt to be in the packaged file list.");
            });
        }

        /// <summary>
        /// Verifies Nuget package created by dotnet pack command. Checks for nupkg in bin/configration folder.
        /// Verifies DACPAC is in the tools folder within the nupkg, and verifies DACPAC is one of its package types.
        /// </summary>
        /// <param name="packageNameOverride">When not set, defaults to Name.Version.nupkg</param>
        /// <param name="test">Additional test to run against the package</param>
        private void VerifyNugetPackage(string packageNameOverride = "", Action<PackageArchiveReader>? test = null)
        {
            // Verify packakge exists
            string packageName = String.IsNullOrEmpty(packageNameOverride) ? $"{DatabaseProjectName}.1.0.0.nupkg" : packageNameOverride;
            string packagePath = Path.Combine(this.GetOutputDirectory(), packageName);
            Assert.IsTrue(File.Exists(packagePath), $"Nuget package not found: {packagePath}");

            using var packageReader = new PackageArchiveReader(packagePath);

            // Verify dacpac is in tools folder
            var files = packageReader.GetFiles();
            Assert.IsTrue(files.Any(f => f.Equals($"tools/{DatabaseProjectName}.dacpac", StringComparison.OrdinalIgnoreCase)),
                $"Expected 'tools/{DatabaseProjectName}.dacpac' to be in the Nuget package.");

            // Verify the DACPAC package type is set
            var packageTypes = packageReader.GetPackageTypes();
            Assert.IsTrue(packageTypes.Any(t => t.Name.Equals("DACPAC", StringComparison.OrdinalIgnoreCase)),
                "Expected 'DACPAC' to be one of the package types.");

            // Run additional test on the package
            test?.Invoke(packageReader);
        }
    }
}