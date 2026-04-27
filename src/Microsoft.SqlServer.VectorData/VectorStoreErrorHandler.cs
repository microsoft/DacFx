// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.VectorData;

#pragma warning disable MEVD9000 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// Contains helpers for reading vector store model properties and their attributes.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class VectorStoreErrorHandler
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<TResult> RunOperationAsync<TResult, TException>(
        VectorStoreMetadata metadata,
        string operationName,
        Func<Task<TResult>> operation)
        where TException : Exception
    {
        return RunOperationAsync<TResult, TException>(
            new VectorStoreCollectionMetadata()
            {
                CollectionName = null,
                VectorStoreName = metadata.VectorStoreName,
                VectorStoreSystemName = metadata.VectorStoreSystemName,
            },
            operationName,
            operation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<TResult> RunOperationAsync<TResult, TException>(
        VectorStoreCollectionMetadata metadata,
        string operationName,
        Func<Task<TResult>> operation)
        where TException : Exception
    {
        try
        {
            return await operation.Invoke().ConfigureAwait(false);
        }
        catch (AggregateException ex) when (ex.InnerException is TException innerEx)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
        catch (TException ex)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult RunOperation<TResult, TException>(
        VectorStoreMetadata metadata,
        string operationName,
        Func<TResult> operation)
        where TException : Exception
    {
        return RunOperation<TResult, TException>(
            new VectorStoreCollectionMetadata()
            {
                CollectionName = null,
                VectorStoreName = metadata.VectorStoreName,
                VectorStoreSystemName = metadata.VectorStoreSystemName,
            },
            operationName,
            operation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult RunOperation<TResult, TException>(
        VectorStoreCollectionMetadata metadata,
        string operationName,
        Func<TResult> operation)
        where TException : Exception
    {
        try
        {
            return operation.Invoke();
        }
        catch (AggregateException ex) when (ex.InnerException is TException innerEx)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
        catch (TException ex)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task RunOperationAsync<TException>(
        VectorStoreCollectionMetadata metadata,
        string operationName,
        Func<Task> operation)
        where TException : Exception
    {
        try
        {
            await operation.Invoke().ConfigureAwait(false);
        }
        catch (AggregateException ex) when (ex.InnerException is TException innerEx)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
        catch (TException ex)
        {
            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
    }

    internal static Task<bool> ReadWithErrorHandlingAsync(
        this DbDataReader reader,
        VectorStoreCollectionMetadata metadata,
        string operationName,
        CancellationToken cancellationToken)
        => VectorStoreErrorHandler.RunOperationAsync<bool, DbException>(
            metadata,
            operationName,
            () => reader.ReadAsync(cancellationToken));

    internal static Task<bool> ReadWithErrorHandlingAsync(
        this DbDataReader reader,
        VectorStoreMetadata metadata,
        string operationName,
        CancellationToken cancellationToken)
        => VectorStoreErrorHandler.RunOperationAsync<bool, DbException>(
            metadata,
            operationName,
            () => reader.ReadAsync(cancellationToken));

    internal static async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(
        this DbConnection connection,
        VectorStoreMetadata metadata,
        string operationName,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        return await ExecuteWithErrorHandlingAsync(
            connection,
            new VectorStoreCollectionMetadata
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = null
            },
            operationName,
            operation,
            cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<TResult> ExecuteWithErrorHandlingAsync<TResult>(
        this DbConnection connection,
        VectorStoreCollectionMetadata metadata,
        string operationName,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (DbException ex)
        {
#if NET
            await connection.DisposeAsync().ConfigureAwait(false);
#else
            connection.Dispose();
#endif

            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
        catch (IOException ex)
        {
#if NET
            await connection.DisposeAsync().ConfigureAwait(false);
#else
            connection.Dispose();
#endif

            throw new VectorStoreException("Call to vector store failed.", ex)
            {
                VectorStoreSystemName = metadata.VectorStoreSystemName,
                VectorStoreName = metadata.VectorStoreName,
                CollectionName = metadata.CollectionName,
                OperationName = operationName
            };
        }
        catch (Exception)
        {
#if NET
            await connection.DisposeAsync().ConfigureAwait(false);
#else
            connection.Dispose();
#endif
            throw;
        }
    }
}
