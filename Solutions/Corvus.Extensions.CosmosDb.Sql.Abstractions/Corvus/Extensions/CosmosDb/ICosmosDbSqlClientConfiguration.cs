// <copyright file="ICosmosDbSqlClientConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using Corvus.Extensions.Json;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// Encapsulates a complete CosmosDB client configuration.
    /// </summary>
    public interface ICosmosDbSqlClientConfiguration
    {
        /// <summary>
        /// Gets or sets the account URI.
        /// </summary>
        Uri AccountUri { get; set; }

        /// <summary>
        /// Gets or sets the connection policy.
        /// </summary>
        ConnectionPolicy ConnectionPolicy { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified in
        /// <see cref="CosmosDbSqlClientDefinition.Container"/>.
        /// </summary>
        string Container { get; set; }

        /// <summary>
        /// Gets or sets the database name. If set, this overrides the name specified in
        /// <see cref="CosmosDbSqlClientDefinition.Database"/>.
        /// </summary>
        string Database { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RU Per Minute Throughput is offered by default.
        /// </summary>
        bool DefaultOfferEnableRUPerMinuteThroughput { get; set; }

        /// <summary>
        /// Gets or sets the default offer for throughput.
        /// </summary>
        int DefaultOfferThroughput { get; set; }

        /// <summary>
        /// Gets or sets the default time to live for a document.
        /// </summary>
        int? DefaultTimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the desired consistency level.
        /// </summary>
        ConsistencyLevel? DesiredConsistencyLevel { get; set; }

        /// <summary>
        /// Gets or sets the indexing policy.
        /// </summary>
        IndexingPolicy IndexingPolicy { get; set; }

        /// <summary>
        /// Gets or sets the partition key definition.
        /// </summary>
        PartitionKeyDefinition PartitionKeyDefinition { get; set; }

        /// <summary>
        /// Gets or sets the collection of properties for this configuration.
        /// </summary>
        PropertyBag Properties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether key rotation is supported.
        /// </summary>
        bool SupportKeyRotation { get; set; }

        /// <summary>
        /// Gets or sets the unique key policy.
        /// </summary>
        UniqueKeyPolicy UniqueKeyPolicy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use database-level throughput.
        /// </summary>
        bool UseDatabaseThroughput { get; set; }

        /// <summary>
        /// Set a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key for the property.</param>
        /// <param name="value">The value of the property.</param>
        void SetProperty<T>(string key, T value);

        /// <summary>
        /// Get a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the object was found.</returns>
        bool TryGetProperty<T>(string key, out T result);
    }
}