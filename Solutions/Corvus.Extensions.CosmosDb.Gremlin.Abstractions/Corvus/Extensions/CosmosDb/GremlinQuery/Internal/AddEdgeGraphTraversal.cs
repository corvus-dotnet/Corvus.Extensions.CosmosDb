// <copyright file="AddEdgeGraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    internal class AddEdgeGraphTraversal<TStart> : GraphTraversal<TStart, Edge>
    {
        private readonly string edgeLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddEdgeGraphTraversal{TStart}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="edgeLabel">The label of the edge.</param>
        public AddEdgeGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string edgeLabel)
            : base(client, parent)
        {
            this.edgeLabel = edgeLabel;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("addE(").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.edgeLabel).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}