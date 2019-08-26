// <copyright file="AddVertexGraphTraversal.cs" company="Endjin Limited">
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
    /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
    /// <typeparam name="TVertex">The type of the vertex it produces.</typeparam>
    internal class AddVertexGraphTraversal<TStart, TEnd, TVertex> : GraphTraversal<TStart, TVertex>
    {
        private readonly string vertexLabel;
        private readonly TVertex vertex;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddVertexGraphTraversal{TStart, TEnd, TVertex}"/> class.
        /// </summary>
        /// <param name="client">The client for which this is a traversal.</param>
        /// <param name="parent">The parent traversal.</param>
        /// <param name="vertexLabel">The vertex label.</param>
        /// <param name="vertex">The vertex.</param>
        public AddVertexGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string vertexLabel = null, TVertex vertex = default)
            : base(client, parent)
        {
            this.vertexLabel = vertexLabel;
            this.vertex = vertex;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(this.vertexLabel))
            {
                await writer.WriteAsync("addV()").ConfigureAwait(false);
            }
            else
            {
                string bindingName = GetBindingName(bindings);
                await writer.WriteAsync("addV(").ConfigureAwait(false);
                await writer.WriteAsync(bindingName).ConfigureAwait(false);
                await writer.WriteAsync(")").ConfigureAwait(false);
                bindings.Add(bindingName, this.vertexLabel);
            }

            await this.WriteProperties(this.vertex, writer, bindings, includeIdAndPartitionKey: true).ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}