// <copyright file="HasPropertyPredicateGraphTraversal.cs" company="Endjin Limited">
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
    internal class HasPropertyPredicateGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string propertyKey;
        private readonly string propertyPredicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HasPropertyPredicateGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="propertyKey">The property to test.</param>
        /// <param name="propertyPredicate">The value of the property to test.</param>
        public HasPropertyPredicateGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string propertyKey, string propertyPredicate)
            : base(client, parent)
        {
            this.propertyKey = propertyKey;
            this.propertyPredicate = propertyPredicate;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("has(").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.propertyKey).ConfigureAwait(false);
            await writer.WriteAsync(",").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.propertyPredicate).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
        }
    }
}