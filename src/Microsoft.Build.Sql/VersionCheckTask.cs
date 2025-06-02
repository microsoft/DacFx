// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.Build.Framework;
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
            // Check for prerelease versions if current version is a prerelease version, otherwise check for stable versions only.
            NuGetVersion latestVersion = NuGetClient.GetLatestPackageVersion(
                PackageName,
                currentVersion.IsPrerelease,
                _cancellationTokenSource.Token).GetAwaiter().GetResult();

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

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
}