// <copyright file="ToGraphTraversal.cs" company="Endjin Limited">
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
    /// <typeparam name="TEnd">The end of the traversal.</typeparam>
    internal class ToGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string vertexId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="vertexId">The id of the target vertex.</param>
        public ToGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string vertexId)
            : base(client, parent)
        {
            this.vertexId = vertexId;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("to(g.V(").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.vertexId).ConfigureAwait(false);
            await writer.WriteAsync("))").ConfigureAwait(false);
        }
    }
}