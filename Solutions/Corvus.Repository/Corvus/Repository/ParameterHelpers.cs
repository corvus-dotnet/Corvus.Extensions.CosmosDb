// <copyright file="ParameterHelpers.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Repository
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Corvus.Extensions;
    using Microsoft.Azure.Documents;

    /// <summary>
    /// Utilities to help build a <see cref="SqlParameterCollection"/> from <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class ParameterHelpers
    {
        /// <summary>
        /// Get a <see cref="SqlParameterCollection"/> for the values.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="values">The <see cref="IEnumerable{T}"/> values.</param>
        /// <returns>A <see cref="SqlParameterCollection"/> of items named <c>@value0,@value1...@valueN</c> with the corresponding value from the enumerable.</returns>
        /// <remarks>This returns an empty parameter collection if the enumerable was null.</remarks>
        public static SqlParameterCollection GetParameterCollection<T>(IEnumerable<T> values)
        {
            var result = new SqlParameterCollection();
            values?.ForEachAtIndex((v, i) => result.Add(new SqlParameter($"@value{i + 1}", v)));

            return result;
        }

        /// <summary>
        /// Gets a string representing a comma-separated list of values for insertion into a SQL query.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="values">The <see cref="IEnumerable{T}"/> of values.</param>
        /// <returns>A comma-separated string of the form <c>@value1, @value2, ..., @valueN</c>.</returns>
        public static string GetValueList<T>(IEnumerable<T> values)
        {
            var result = new StringBuilder();
            Enumerable.Range(1, values.Count()).ForEach(v =>
            {
                if (result.Length > 0)
                {
                    result.Append(", ");
                }

                result.Append("@value").Append(v);
            });

            return result.ToString();
        }
    }
}
