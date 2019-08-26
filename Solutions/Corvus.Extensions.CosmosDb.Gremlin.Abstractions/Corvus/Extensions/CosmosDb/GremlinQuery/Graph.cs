// <copyright file="Graph.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A root traversal for a graph.
    /// </summary>
    public class Graph : GraphTraversal<dynamic, dynamic>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        /// <param name="client">The client of which this is a root.</param>
        public Graph(ICosmosDbGremlinClient client)
            : base(client, null)
        {
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            await writer.WriteAsync("g").ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}
