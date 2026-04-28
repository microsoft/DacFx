// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Data.SqlClient;
using Xunit;

namespace SqlServer.ConformanceTests.Support;

/// <summary>
/// Helpers for tests that require Azure SQL Database or SQL database in Microsoft Fabric.
/// </summary>
internal static class AzureSqlHelper
{
    private static bool? s_isAzureSql;

    public static async Task EnsureAzureSqlAsync()
    {
        if (s_isAzureSql is not null)
        {
            Assert.True(s_isAzureSql.Value, "This test requires Azure SQL Database or SQL database in Microsoft Fabric.");
            return;
        }

        var connectionString = SqlServerTestStore.Instance.ConnectionString;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT SERVERPROPERTY('EngineEdition')";
        var result = await command.ExecuteScalarAsync();
        var engineEdition = Convert.ToInt32(result);

        // 5 = Azure SQL Database, 11 = SQL database in Microsoft Fabric
        s_isAzureSql = engineEdition is 5 or 11;

        Assert.True(s_isAzureSql.Value, "This test requires Azure SQL Database or SQL database in Microsoft Fabric.");
    }
}
