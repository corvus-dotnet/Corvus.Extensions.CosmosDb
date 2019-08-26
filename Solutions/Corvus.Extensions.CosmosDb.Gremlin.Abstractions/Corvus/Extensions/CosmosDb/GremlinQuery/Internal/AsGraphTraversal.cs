// <copyright file="AsGraphTraversal.cs" company="Endjin Limited">
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
    internal class AsGraphTraversal<TStart, TEnd> : GraphTraversal<TStart, TEnd>
    {
        private readonly string stepLabel;
        private readonly string[] stepLabels;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="stepLabel">The step label.</param>
        /// <param name="stepLabels">Additional step labels.</param>
        public AsGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string stepLabel, string[] stepLabels)
            : base(client, parent)
        {
            this.stepLabel = stepLabel;
            this.stepLabels = stepLabels;
        }

        /// <inheritdoc />
        public override async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            if (this.Parent != null)
            {
                await writer.WriteAsync(".").ConfigureAwait(false);
            }

            await writer.WriteAsync("as(").ConfigureAwait(false);
            string labelName = GetBindingName(bindings);
            bindings.Add(labelName, this.stepLabel);
            await writer.WriteAsync(labelName).ConfigureAwait(false);

            for (int i = 0; i < this.stepLabels.Length; ++i)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
                string name = GetBindingName(bindings);
                bindings.Add(name, this.stepLabels[i]);
                await writer.WriteAsync(name).ConfigureAwait(false);
            }

            await writer.WriteAsync(")").ConfigureAwait(false);

            await base.WriteAsync(writer, bindings).ConfigureAwait(false);
        }
    }
}