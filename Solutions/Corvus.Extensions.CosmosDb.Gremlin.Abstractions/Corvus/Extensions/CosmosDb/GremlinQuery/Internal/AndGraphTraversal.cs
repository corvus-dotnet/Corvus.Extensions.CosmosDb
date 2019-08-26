// <copyright file="AndGraphTraversal.cs" company="Endjin Limited">
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
    internal class AndGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly ITraversal[] andTraversals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="andTraversals">The set of traversals which must all yield a result.</param>
        public AndGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, ITraversal[] andTraversals)
            : base(client, parent)
        {
            this.andTraversals = andTraversals;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("and(").ConfigureAwait(false);
            for (int i = 0; i < this.andTraversals.Length; ++i)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(",").ConfigureAwait(false);
                }

                await this.andTraversals[i].WriteAsync(writer, bindings).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}