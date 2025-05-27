// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace Microsoft.Build.Sql;

public static class NugetClient
{
    private const string CacheFileName = "VersionCache.json";
    private const int CacheFileExpirationInDays = 7;
    private const int HttpClientTimeoutInSeconds = 5;

    /// <summary>
    /// Factory method to create an HttpClient instance, defaults to a new instance with a 5-second timeout.
    /// </summary>
    public static Func<HttpClient> HttpClientFactory { get; set; } = () => new HttpClient() { Timeout = TimeSpan.FromSeconds(HttpClientTimeoutInSeconds) };

    /// <summary>
    /// Gets the latest version of given package. If there is a cached version, it will be used if not expired.
    /// If the cache is expired or does not exist, it will fetch the latest version from NuGet.org.
    /// </summary>
    public static async Task<NuGetVersion> GetLatestPackageVersion(string packageName, bool prerelease, CancellationToken cancellationToken = default)
    {
        // Check if we have a cached version
        NuGetVersionsData versionsData = await GetCachedVersionDataAsync(packageName, cancellationToken);

        // If not, fetch the latest version from NuGet.org
        if (versionsData == null)
        {
            // API: https://learn.microsoft.com/nuget/api/package-base-address-resource#enumerate-package-versions
            string url = $"https://api.nuget.org/v3-flatcontainer/{packageName.ToLowerInvariant()}/index.json";

            using HttpClient client = HttpClientFactory();
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            versionsData = await JsonSerializer.DeserializeAsync<NuGetVersionsData>(jsonStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

            // Cache the result
            if (versionsData != null)
            {
                await WriteCacheAsync(packageName, versionsData, cancellationToken);
            }
        }

        // Parse the versions and filter based on prerelease flag
        return versionsData?.Versions
            .Select(v => NuGetVersion.Parse(v))
            .Where(v => prerelease || !v.IsPrerelease)
            .OrderByDescending(v => v)
            .FirstOrDefault() ?? throw new InvalidOperationException("No versions found for the package.");
    }

    /// <summary>
    /// Gets the cached version data for the given package name.
    /// Returns null if the cache is expired or does not exist.
    /// </summary>
    private static async Task<NuGetVersionsData> GetCachedVersionDataAsync(string packageName, CancellationToken cancellationToken)
    {
        string cacheFilePath = GetVersionCacheFilePath(packageName);
        if (File.Exists(cacheFilePath))
        {
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(cacheFilePath);
            if (DateTime.UtcNow - lastWriteTime < TimeSpan.FromDays(CacheFileExpirationInDays))
            {
                await using FileStream fileStream = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return await JsonSerializer.DeserializeAsync<NuGetVersionsData>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            }
            else
            {
                File.Delete(cacheFilePath); // Delete expired cache
            }
        }

        return null;
    }

    private static async Task WriteCacheAsync(string packageName, NuGetVersionsData cacheData, CancellationToken cancellationToken)
    {
        string cacheFilePath = GetVersionCacheFilePath(packageName);
        await using FileStream fileStream = new FileStream(cacheFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fileStream, cacheData, cancellationToken: cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the file path for the cache file containing the version data.
    /// </summary>
    public static string GetVersionCacheFilePath(string packageName)
    {
        string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), packageName);
        Directory.CreateDirectory(cacheDirectory);
        return Path.Combine(cacheDirectory, CacheFileName);
    }

    /// <summary>
    /// JSON data structure for NuGet versions
    /// </summary>
    private class NuGetVersionsData
    {
        public required string[] Versions { get; set; }
    }
}