// <copyright file="HasLabelGraphTraversal.cs" company="Endjin Limited">
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
    internal class HasLabelGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string label;

        /// <summary>
        /// Initializes a new instance of the <see cref="HasLabelGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="label">The label to query.</param>
        public HasLabelGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string label)
            : base(client, parent)
        {
            this.label = label;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("hasLabel(").ConfigureAwait(false);
            await WriteBoundValueAsync(writer, bindings, this.label).ConfigureAwait(false);
            await writer.WriteAsync(")").ConfigureAwait(false);
        }
    }
}