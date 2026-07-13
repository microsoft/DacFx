// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.SqlServer.Server;

public partial class StoredProcedures
{
    [SqlProcedure]
    public static void HelloWorld()
    {
        SqlContext.Pipe.Send("Hello from CLR!");
    }
}
