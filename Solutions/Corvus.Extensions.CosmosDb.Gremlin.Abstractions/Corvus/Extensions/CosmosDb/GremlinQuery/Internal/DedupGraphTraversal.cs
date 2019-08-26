// <copyright file="DedupGraphTraversal.cs" company="Endjin Limited">
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
    internal class DedupGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string[] dedupLabels;

        /// <summary>
        /// Initializes a new instance of the <see cref="DedupGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="dedupLabels">If labels are provided, then the scoped object's labels determine de-duplication. No labels implies current object.</param>
        public DedupGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string[] dedupLabels)
            : base(client, parent)
        {
            this.dedupLabels = dedupLabels;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("dedup(").ConfigureAwait(false);

            for (int i = 0; i < this.dedupLabels.Length; ++i)
            {
                if (i != 0)
                {
                    await writer.WriteAsync(",").ConfigureAwait(false);
                }

                string name = GetBindingName(bindings);
                bindings.Add(name, this.dedupLabels[i]);
                await writer.WriteAsync(name).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);
        }
    }
}