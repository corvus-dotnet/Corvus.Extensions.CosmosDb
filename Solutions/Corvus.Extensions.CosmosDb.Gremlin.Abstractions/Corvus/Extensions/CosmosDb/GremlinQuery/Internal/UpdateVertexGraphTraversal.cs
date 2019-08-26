// <copyright file="UpdateVertexGraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    /// <typeparam name="TEnd">The type of vertex to find.</typeparam>
    public class UpdateVertexGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly TEnd vertex;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateVertexGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="vertex">The vertex to update.</param>
        public UpdateVertexGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, TEnd vertex)
            : base(client, parent)
        {
            this.vertex = vertex;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            string idName = GetBindingName(bindings);
            bindings.Add(idName, string.Empty);
            await writer.WriteAsync("V(").ConfigureAwait(false);
            await writer.WriteAsync(idName).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
            string partitionKeyBindingName = null;
            if (this.Client.PartitionKeyDefinition?.Paths?.Count > 0)
            {
                string partitionKeyPropertyName = this.Client.PartitionKeyDefinition.Paths.Single().Trim('/');
                partitionKeyBindingName = GetBindingName(bindings);
                bindings.Add(partitionKeyBindingName, string.Empty);
                await writer.WriteAsync(".has('").ConfigureAwait(false);
                await writer.WriteAsync(partitionKeyPropertyName).ConfigureAwait(false);
                await writer.WriteAsync("',").ConfigureAwait(false);
                await writer.WriteAsync(partitionKeyBindingName).ConfigureAwait(false);
                await writer.WriteAsync(")").ConfigureAwait(false);
            }

            (string id, string partitionKey) = await this.WriteProperties(this.vertex, writer, bindings, includeIdAndPartitionKey: false).ConfigureAwait(false);
            bindings[idName] = id;
            if (partitionKeyBindingName != null)
            {
                bindings[partitionKeyBindingName] = partitionKey;
            }

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}