// <copyright file="ValueUtilities.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosClient.Specs.Common.Driver
{
    using System;

    internal static class ValueUtilities
    {
        /// <summary>
        /// Gets a nullable string from a string.
        /// </summary>
        /// <param name="nullableString">The string value, or the string <c>null</c> for null.</param>
        /// <returns>A string, or null if the input string contained <c>null</c>.</returns>
        internal static string? GetNullableString(string nullableString)
        {
            return nullableString == "null" ? null : nullableString;
        }

        /// <summary>
        /// Gets a nullable DateTimeOffset from a string.
        /// </summary>
        /// <param name="nullableDateTimeOffsetString">The string value that can be parsed to a <see cref="DateTimeOffset"/>, or the string <c>null</c> for null.</param>
        /// <returns>The <see cref="DateTimeOffset"/> represented by the input string, or null if it contained <c>null</c>.</returns>
        internal static DateTimeOffset? GetNullableDateTimeOffset(string nullableDateTimeOffsetString)
        {
            return nullableDateTimeOffsetString == "null" ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableDateTimeOffsetString);
        }
    }
}
