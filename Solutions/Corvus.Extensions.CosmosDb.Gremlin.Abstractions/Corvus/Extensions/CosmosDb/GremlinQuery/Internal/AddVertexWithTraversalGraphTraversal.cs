// <copyright file="AddVertexWithTraversalGraphTraversal.cs" company="Endjin Limited">
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
    /// <typeparam name="TEnd">The type of the vertex produced.</typeparam>
    /// <typeparam name="TLabelStart">The start of the traversal used to produce the vertex label.</typeparam>
    internal class AddVertexWithTraversalGraphTraversal<TStart, TEnd, TLabelStart> : GraphTraversal<TStart, TEnd>
    {
        private readonly ITraversal<TLabelStart, string> vertexLabel;
        private readonly TEnd vertex;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddVertexWithTraversalGraphTraversal{TStart, TEnd, TLabelStart}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="vertexLabel">The label for the vertex.</param>
        /// <param name="vertex">The vertex to add.</param>
        public AddVertexWithTraversalGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, ITraversal<TLabelStart, string> vertexLabel = null, TEnd vertex = default)
            : base(client, parent)
        {
            this.vertexLabel = vertexLabel;
            this.vertex = vertex;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("addV(").ConfigureAwait(false);
            await this.vertexLabel.WriteAsync(writer, bindings).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);

            await this.WriteProperties(this.vertex, writer, bindings, includeIdAndPartitionKey: true).ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}