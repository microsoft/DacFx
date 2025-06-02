// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Microsoft.Build.Sql.Tests;

public class NuGetClientTests
{
    private const string PackageName = "Microsoft.Build.Sql";

    [Test]
    public async Task TestGetLatestVersion()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"versions\": [\"1.0.0\", \"1.0.1\", \"1.0.2\", \"1.0.3-preview\"]}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        NuGetClient.HttpClientFactory = () => httpClient;

        // Act
        var sdkVersion = await NuGetClient.GetLatestPackageVersion(PackageName, false);

        // Assert
        Assert.AreEqual("1.0.2", sdkVersion.ToString(), "Latest version should be 1.0.2");
        FileAssert.Exists(NuGetClient.GetVersionCacheFilePath(PackageName), "Cache file should exist after fetching the version.");
    }

    [Test]
    public async Task GetLatestVersionPrerelease()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"versions\": [\"1.0.0\", \"1.0.1\", \"1.0.2\", \"1.0.3-preview\"]}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        NuGetClient.HttpClientFactory = () => httpClient;

        // Act
        var sdkVersion = await NuGetClient.GetLatestPackageVersion(PackageName, true);

        // Assert
        Assert.AreEqual("1.0.3-preview", sdkVersion.ToString(), "Latest version should be 1.0.3-preview");
    }

    [Test]
    public void GetLatestVersionWithHttpFailure()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        NuGetClient.HttpClientFactory = () => httpClient;

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await NuGetClient.GetLatestPackageVersion(PackageName, false));
    }

    [Test]
    public void GetLatestVersionWithJsonFailure()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Invalid JSON")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        NuGetClient.HttpClientFactory = () => httpClient;

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await NuGetClient.GetLatestPackageVersion(PackageName, false));
    }

    [Test]
    public async Task GetLatestVersionWithCache()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"versions\": [\"1.0.0\", \"1.0.1\", \"1.0.2\", \"1.0.3-preview\"]}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        NuGetClient.HttpClientFactory = () => httpClient;

        // Act
        var sdkVersion = await NuGetClient.GetLatestPackageVersion(PackageName, false);

        // Assert
        Assert.AreEqual("1.0.2", sdkVersion.ToString(), "Latest version should be 1.0.2");

        // Call again to make sure it uses the cache, this time with prerelease version
        sdkVersion = await NuGetClient.GetLatestPackageVersion(PackageName, true);
        Assert.AreEqual("1.0.3-preview", sdkVersion.ToString(), "Latest version should be 1.0.3-preview");

        // Verify that the HTTP request was made only once
        mockHttpMessageHandler.Protected()
            .Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [SetUp]
    public void TestSetUp()
    {
        DeleteCache();
    }

    [TearDown]
    public void TestTearDown()
    {
        DeleteCache();
    }

    private void DeleteCache()
    {
        // Delete the cached version file before and after each test run
        string cacheFilePath = NuGetClient.GetVersionCacheFilePath(PackageName);
        if (File.Exists(cacheFilePath))
        {
            File.Delete(cacheFilePath);
        }
    }
}
