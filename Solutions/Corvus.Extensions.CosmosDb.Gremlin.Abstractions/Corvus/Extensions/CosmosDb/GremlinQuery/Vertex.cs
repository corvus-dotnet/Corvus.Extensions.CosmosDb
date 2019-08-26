// <copyright file="Vertex.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// A default vertex in the graph.
    /// </summary>
    public class Vertex
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
