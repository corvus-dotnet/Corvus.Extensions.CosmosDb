// <copyright file="GetVertexGraphTraversal.cs" company="Endjin Limited">
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
    /// <typeparam name="TEnd">The type of vertex produced by the traversal.</typeparam>
    internal class GetVertexGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string[] ids;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetVertexGraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="ids">The vertex ids.</param>
        public GetVertexGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string[] ids)
            : base(client, parent)
        {
            this.ids = ids;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("V(").ConfigureAwait(false);
            for (int i = 0; i < this.ids.Length; ++i)
            {
                if (i > 0)
                {
                    await writer.WriteAsync(",").ConfigureAwait(false);
                }

                string bindingName = GetBindingName(bindings);
                bindings.Add(bindingName, this.ids[i]);
                await writer.WriteAsync(bindingName).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}