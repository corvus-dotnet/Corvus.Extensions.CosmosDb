// <copyright file="ITraversalWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Writes a traversal to an output.
    /// </summary>
    public interface ITraversalWriter : IDisposable
    {
        /// <summary>
        /// Writes a traversal to the output.
        /// </summary>
        /// <param name="traversalContent">The content of the traversal.</param>
        /// <returns>A <see cref="Task"/> which completes when the content is written.</returns>
        Task WriteAsync(string traversalContent);
    }
}
