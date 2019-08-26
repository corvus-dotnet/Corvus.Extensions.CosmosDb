// <copyright file="GremlinPredicates.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System.Collections.Generic;

    /// <summary>
    /// Standard predicates for gremlin queries.
    /// </summary>
    public static class GremlinPredicates
    {
        /// <summary>
        /// Determines if a value is within (inclusive) of the range of the two specified values.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to compare.</param>
        /// <param name="min">The minimum value (inclusive).</param>
        /// <param name="max">The maximum value (inclusive).</param>
        /// <returns>True if the values is within the range of the minimum and maximum values (inclusive).</returns>
        public static bool Between<T>(this T value, T min, T max)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            return comparer.Compare(value, min) >= 0 && comparer.Compare(value, max) <= 0;
        }

        /// <summary>
        /// Determines if a value is within (exclusive) the range of the two specified values.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to compare.</param>
        /// <param name="min">The minimum value (exclusive).</param>
        /// <param name="max">The maximum value (exclusive).</param>
        /// <returns>True if the values is within the range of the minimum and maximum values (exclusive).</returns>
        public static bool Inside<T>(this T value, T min, T max)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            return comparer.Compare(value, min) > 0 && comparer.Compare(value, max) < 0;
        }
    }
}
