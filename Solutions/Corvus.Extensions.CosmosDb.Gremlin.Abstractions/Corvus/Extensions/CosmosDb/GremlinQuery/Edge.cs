// <copyright file="Edge.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// A standard edge in the graph.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Gets or sets the ID of the edge.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the edge.
        /// </summary>
        public string Label { get; set; }
    }
}
