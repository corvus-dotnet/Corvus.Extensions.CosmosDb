// <copyright file="FoldGraphTraversal.cs" company="Endjin Limited">
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
    internal class FoldGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly TEnd seed;
        private readonly string foldFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoldGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="seed">The seed.</param>
        /// <param name="foldFunction">The fold function.</param>
        public FoldGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, TEnd seed, string foldFunction)
            : base(client, parent)
        {
            this.seed = seed;
            this.foldFunction = foldFunction;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("fold(").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.seed).ConfigureAwait(false);
            await writer.WriteAsync(",").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.foldFunction).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
        }
    }
}