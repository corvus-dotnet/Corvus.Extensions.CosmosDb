// <copyright file="AddEdgeWithTraversalGraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of this traversal.</typeparam>
    /// <typeparam name="TLabelStart">The start of the traversal used to find the edge label.</typeparam>
    internal class AddEdgeWithTraversalGraphTraversal<TStart, TLabelStart> : GraphTraversal<TStart, Edge>
    {
        private readonly ITraversal<TLabelStart, string> edgeLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddEdgeWithTraversalGraphTraversal{TStart, TLabelStart}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="edgeLabel">The label for the edge.</param>
        public AddEdgeWithTraversalGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, ITraversal<TLabelStart, string> edgeLabel)
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
            await this.edgeLabel.WriteAsync(writer, bindings).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}