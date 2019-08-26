﻿// <copyright file="OutGraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery.Internal
{
    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    /// <typeparam name="TEnd">The end of the traversal.</typeparam>
    internal class OutGraphTraversal<TStart, TEnd> : OutGraphTraversal<TStart, TEnd, Vertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutGraphTraversal{TStart, TEnd}" /> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        /// <param name="edgeLabels">The id of the target vertex.</param>
        public OutGraphTraversal(ICosmosDbGremlinClient client, ITraversal parent, string[] edgeLabels)
            : base(client, parent, edgeLabels)
        {
        }
    }
}