// <copyright file="ByGraphTraversal{TStart,TEnd,TTraversalStart,TTraversalEnd}.cs" company="Endjin Limited">
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
    /// <typeparam name="TTraversalStart">The start of the traversal providing the ordering.</typeparam>
    /// <typeparam name="TTraversalEnd">The end of the traversal providing the ordering.</typeparam>
    internal class ByGraphTraversal<TStart, TEnd, TTraversalStart, TTraversalEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly ITraversal<TTraversalStart, TTraversalEnd> traversal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByGraphTraversal{TStart, TEnd, TTraversalStart, TTraversalEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="traversal">The traversal.</param>
        public ByGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, ITraversal<TTraversalStart, TTraversalEnd> traversal)
            : base(client, parent)
        {
            this.traversal = traversal;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("by(").ConfigureAwait(false);
            await this.traversal.WriteAsync(writer, bindings).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}