// <copyright file="CoalesceGraphTraversal.cs" company="Endjin Limited">
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
    internal class CoalesceGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly ITraversal[] traversals;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoalesceGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="traversals">The set of traversals which must all yield a result.</param>
        public CoalesceGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, ITraversal<TEnd>[] traversals)
            : base(client, parent)
        {
            this.traversals = traversals;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("coalesce(").ConfigureAwait(false);
            for (int i = 0; i < this.traversals.Length; ++i)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(",").ConfigureAwait(false);
                }

                await this.traversals[i].WriteAsync(writer, bindings).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}