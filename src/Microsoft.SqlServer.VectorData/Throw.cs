// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable CA1716
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting
namespace Microsoft.Shared.Diagnostics;
#pragma warning restore CA1716

/// <summary>
/// Defines static methods used to throw exceptions.
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class Throw
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T IfNull<T>([NotNull] T argument, [CallerArgumentExpression(nameof(argument))] string paramName = "")
    {
        if (argument is null)
        {
            ArgumentNullException(paramName);
        }

        return argument;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static string IfNullOrWhitespace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string paramName = "")
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            if (argument == null)
            {
                ArgumentNullException(paramName);
            }
            else
            {
                ArgumentException(paramName, "Argument is whitespace");
            }
        }

        return argument!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IfLessThan(int argument, int min, [CallerArgumentExpression(nameof(argument))] string paramName = "")
    {
        if (argument < min)
        {
            ArgumentOutOfRangeException(paramName, argument, $"Argument less than minimum value {min}");
        }

        return argument;
    }

    [DoesNotReturn]
    public static void ArgumentNullException(string paramName)
        => throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    public static void ArgumentOutOfRangeException(string paramName, object? actualValue, string? message)
        => throw new ArgumentOutOfRangeException(paramName, actualValue, message);

    [DoesNotReturn]
    public static void ArgumentException(string paramName, string? message)
        => throw new ArgumentException(message, paramName);
}
