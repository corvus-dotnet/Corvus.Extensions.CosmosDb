// <copyright file="ICosmosDbGremlinClient.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb.GremlinQuery;
    using Gremlin.Net.Driver;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Adds a graph API over <see cref="Gremlin.Net.Driver.IGremlinClient"/>, and unifies with the
    /// <see cref="ICosmosDbSqlClient"/>.
    /// </summary>
    public interface ICosmosDbGremlinClient : ICosmosDbSqlClient, IDisposable
    {
        /// <summary>
        /// Gets the gremlin server host name.
        /// </summary>
        string GremlinHost { get; }

        /// <summary>
        /// Gets the gremlin server port.
        /// </summary>
        int GremlinPort { get; }

        /// <summary>
        /// Gets a value indicating whether to use SSL.
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// Gets the Gremlin client for the client.
        /// </summary>
        /// <returns>A <see cref="Task"/> which, when complete, provides the Gremlin client.</returns>
        Task<IGremlinClient> GetGremlinClientAsync();

        /// <summary>
        /// Executes a Gremlin query.
        /// </summary>
        /// <param name="request">A standard Gremlin query message.</param>
        /// <param name="bindings">A set of parameter bindings to use.</param>
        /// <returns>A <see cref="Task"/> which, when complete, provides a read only collection of <see cref="JToken"/>responses.</returns>
        Task<IReadOnlyCollection<JToken>> ExecuteGremlinQueryAsync(string request, Dictionary<string, object> bindings = null);

        /// <summary>
        /// Executes a Gremlin query with a single result.
        /// </summary>
        /// <param name="request">A standard Gremlin query message.</param>
        /// <param name="bindings">A set of parameter bindings to use.</param>
        /// <returns>A <see cref="Task"/> which, when complete, provides a single <see cref="JToken"/> response.</returns>
        Task<JToken> ExecuteGremlinQuerySingleResultAsync(string request, Dictionary<string, object> bindings = null);

        /// <summary>
        /// Starts a traversal from the graph.
        /// </summary>
        /// <returns>A graph traversal source.</returns>
        Graph StartTraversal();
    }
}
