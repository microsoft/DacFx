// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using NuGet.Packaging;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests
{
    [TestFixture]
    public class PackTests : DotnetTestBase
    {
        [Test]
        [Description("Verifies pack with default settings.")]
        public void SuccessfulSimplePack()
        {
            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommand("pack", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyNugetPackage();
        }

        [Test]
        [Description("Verifies packing separately after builid.")]
        public void SuccessfulPackWithNoBuild()
        {
            // Run build first
            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommand("build", out stdOutput, out stdError);
            Assert.AreEqual(0, exitCode, "Build failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();

            // Run pack with --no-build
            exitCode = this.RunDotnetCommand("pack", out stdOutput, out stdError, "--no-build");
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyNugetPackage();
        }

        [Test]
        [Description("Verifies pack with custom defined properties.")]
        public void SuccessfulPackWithCustomProperties()
        {
            // The project file in this test has a bunch of custom properties
            string stdOutput, stdError;
            int exitCode = this.RunDotnetCommand("pack", out stdOutput, out stdError);

            // Verify success
            Assert.AreEqual(0, exitCode, "Pack failed with error " + stdError);
            Assert.AreEqual(string.Empty, stdError);
            this.VerifyDacPackage();
            this.VerifyNugetPackage("", (PackageArchiveReader package) => {
                // TODO
            });
        }

        private string GetDefaultNugetPackagePath(string packageNameOverride = "")
        {
            string packageName = String.IsNullOrEmpty(packageNameOverride) ? $"{DatabaseProjectName}.1.0.0.nupkg" : packageNameOverride;
            return Path.Combine(this.GetOutputDirectory(), packageName);
        }

        private void VerifyNugetPackage(string packageNameOverride = "", Action<PackageArchiveReader>? test = null)
        {
            // Verify packakge exists
            string packagePath = this.GetDefaultNugetPackagePath();
            Assert.IsTrue(File.Exists(packagePath), $"Nuget package not found: {packagePath}");

            using var packageReader = new PackageArchiveReader(packagePath);

            // Verify dacpac is in tools folder
            var files = packageReader.GetFiles();
            Assert.IsTrue(files.Any(f => f.Equals($"tools/{DatabaseProjectName}.dacpac", StringComparison.OrdinalIgnoreCase)));

            // Verify the DACPAC package type is set
            var packageTypes = packageReader.GetPackageTypes();
            Assert.IsTrue(packageTypes.Any(t => t.Name.Equals("DACPAC", StringComparison.OrdinalIgnoreCase)));

            // Run additional test on the package
            test?.Invoke(packageReader);
        }
    }
}