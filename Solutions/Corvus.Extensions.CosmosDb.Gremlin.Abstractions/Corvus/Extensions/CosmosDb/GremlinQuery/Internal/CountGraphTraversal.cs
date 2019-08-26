// <copyright file="CountGraphTraversal.cs" company="Endjin Limited">
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
    internal class CountGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly Scope? scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="scope">The scope on which the count operates.</param>
        public CountGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, Scope? scope = null)
            : base(client, parent)
        {
            this.scope = scope;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            if (this.scope.HasValue)
            {
                await writer.WriteAsync("count(").ConfigureAwait(false);
                await writer.WriteAsync(this.scope.Value == Scope.Global ? "global" : "local").ConfigureAwait(false);
                await writer.WriteAsync(")").ConfigureAwait(false);
            }
            else
            {
                await writer.WriteAsync("count()").ConfigureAwait(false);
            }
        }
    }
}