// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Build-time stubs for SQLCLR types that are not present in the netstandard
// build of Microsoft.SqlServer.Server. They exist so SDK-style SQL projects
// targeting .NET 8.0 can compile user CLR sources (stored procedures,
// triggers, SqlContext / SqlPipe usage). At SQL Server runtime these symbols
// are resolved by the in-server CLR loader against the SQL Server-hosted
// System.Data / Microsoft.SqlServer.Server assemblies, so the bodies here
// intentionally do nothing. See https://github.com/microsoft/DacFx/issues/785

namespace Microsoft.SqlServer.Server
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class SqlProcedureAttribute : System.Attribute
    {
        public string Name { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class SqlTriggerAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string Target { get; set; }
        public string Event { get; set; }
    }

    public static class SqlContext
    {
        public static SqlPipe Pipe { get { return null; } }
        public static SqlTriggerContext TriggerContext { get { return null; } }
        public static bool IsAvailable { get { return false; } }
    }

    public sealed class SqlPipe
    {
        public void Send(string message) { }
    }

    public sealed class SqlTriggerContext
    {
    }
}
