using System.IO;
using Microsoft.SqlServer.Dac;
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

    }
}