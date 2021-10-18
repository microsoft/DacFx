namespace Microsoft.Build.Sql.Tests
{
    public class UnitTestTemplate
    {
        public void TestTemplate()
        {
            // General plan for testing the SDK:
            // 1. After SDK is built, pack it and put the nupkg into a known location (done)
            // 2. Create a test sqlproj that uses the SDK (can be directly checked in)
            // 3. Add nuget.config to sqlproj path, add the nupkg path from #1 to nuget sources
            // 4. Inject the version of the SDK to sqlproj
            // 5. Ensure when the sqlproj is built, it is pulling the correct version of the SDK (we could clear nuget cache)
            // 6. Build the sqlproj and verify dacpac
            // (7.) Verify all SQL objects are present in the dacpac
            // (8.) Publish dacpac and verify against database
        }
    }
}