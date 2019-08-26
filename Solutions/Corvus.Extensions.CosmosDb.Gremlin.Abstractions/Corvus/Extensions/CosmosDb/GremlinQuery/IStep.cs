// <copyright file="IStep.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A step in a traversal.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Writes the step to the output.
        /// </summary>
        /// <param name="writer">The output <see cref="ITraversalWriter"/> to which to write the step.</param>
        /// <param name="bindings">The bindings for the output.</param>
        /// <returns>A <see cref="Task"/> which completes once the output is writter.</returns>
        Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings);
    }
}
