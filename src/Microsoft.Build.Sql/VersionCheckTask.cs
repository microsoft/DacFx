// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Build.Sql;

/// <summary>
/// This task displays a message in the build log if there is a newer version of the package available on NuGet.org.
/// </summary>
public class VersionCheckTask : Microsoft.Build.Utilities.Task, ICancelableTask
{
    /// <summary>
    /// Current version of the package.
    /// </summary>
    [Required]
    public string Version { get; set; } = string.Empty;

    private const string PackageName = "Microsoft.Build.Sql";
    private const string NugetUrl = "https://api.nuget.org/v3/index.json";
    private CancellationTokenSource _cancellationTokenSource = new();

    public override bool Execute()
    {
        if (NuGetVersion.TryParse(Version, out NuGetVersion currentVersion) == false)
        {
            Log.LogWarning($"Invalid NuGetVersion format: {Version}");
            return true;    // This task should not fail build even if the version check fails.
        }

        try
        {
            NuGetVersion latestVersion = GetLatestSdkVersionFromNuGet().Result;
            if (latestVersion > currentVersion)
            {
                Log.LogWarning($"A newer version of {PackageName} is available: {latestVersion}. You are using {currentVersion}.");
            }
            else
            {
                Log.LogMessage(MessageImportance.Low, $"Already using the latest version of {PackageName}: {currentVersion}.");
            }
        }
        catch (Exception ex)
        {
            Log.LogMessage(MessageImportance.Low, $"Failed to check for the latest version of {PackageName} on NuGet: {ex.Message}");
        }

        return true;    // This task should not fail build even if the version check fails.
    }
    
    /// <summary>
    /// Gets the latest version of Microsoft.Build.Sql from NuGet
    /// </summary>
    private async Task<NuGetVersion> GetLatestSdkVersionFromNuGet()
    {
        // Get the metadata resource for the NuGet repository
        var repository = Repository.Factory.GetCoreV3(NugetUrl);
        var resource = await repository.GetResourceAsync<PackageMetadataResource>();

        // Search for the package metadata
        var metadata = await resource.GetMetadataAsync(
            PackageName,
            includePrerelease: true,
            includeUnlisted: false,
            sourceCacheContext: new SourceCacheContext(),
            log: NullLogger.Instance,
            token: _cancellationTokenSource.Token);

        // Extract all versions
        return metadata.Select(m => m.Identity.Version).OrderByDescending(v => v).First();
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
}