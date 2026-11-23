// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

public static class Functions
{
    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static SqlInt32 AddIntegers(SqlInt32 left, SqlInt32 right)
    {
        return left + right;
    }
}
