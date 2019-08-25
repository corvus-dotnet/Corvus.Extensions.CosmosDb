// <copyright file="CosmosDbGremlinSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions.CosmosDb
{
    /// <summary>
    /// Settings for CosmosDB databases and collections used in tests, including Gremlin settings.
    /// </summary>
    public class CosmosDbGremlinSettings : CosmosDbSettings
    {
        /// <summary>
        /// Gets or sets the hostname of the server handling Gremlin (graph) requests.
        /// </summary>
        public string CosmosDbGremlinHost { get; set; }

        /// <summary>
        /// Gets or sets the port number on which the server is handling Gremlin requests.
        /// </summary>
        public int CosmosDbGremlinPort { get; set; }
    }
}
