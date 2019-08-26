// <copyright file="OutGraphTraversal{TStart,TEnd,TVertex}.cs" company="Endjin Limited">
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
    /// <typeparam name="TVertex">The type of the vertex.</typeparam>
    internal class OutGraphTraversal<TStart, TEnd, TVertex> : GraphTraversal<TStart, TVertex>
    {
        private readonly string[] edgeLabels;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutGraphTraversal{TStart, TEnd, TVertex}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="edgeLabels">The id of the target vertex.</param>
        public OutGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string[] edgeLabels)
            : base(client, parent)
        {
            this.edgeLabels = edgeLabels;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("out(").ConfigureAwait(false);
            for (int i = 0; i < this.edgeLabels.Length; ++i)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(",").ConfigureAwait(false);
                }

                string edgeLabel = this.edgeLabels[i];
                await WriteBoundValueAsync(writer, bindings, edgeLabel).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);
        }
    }
}