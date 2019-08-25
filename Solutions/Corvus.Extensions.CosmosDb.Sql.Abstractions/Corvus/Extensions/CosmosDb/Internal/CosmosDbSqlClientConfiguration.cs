// <copyright file="CosmosDbSqlClientConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.Internal
{
    using System;
    using Corvus.Extensions.Json;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Encapsulates a complete client configuration.
    /// </summary>
    public class CosmosDbSqlClientConfiguration : ICosmosDbSqlClientConfiguration
    {
        /// <summary>
        /// The registered content type for the client configuration.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.extensions.cosmosdb.cosmosdbsqlclientconfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClientConfiguration"/> class.
        /// </summary>
        public CosmosDbSqlClientConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClientConfiguration"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use in the context.</param>
        public CosmosDbSqlClientConfiguration(IServiceProvider serviceProvider)
        {
            IJsonSerializerSettingsProvider serializerSettingsProvider = serviceProvider.GetService<IJsonSerializerSettingsProvider>();
            JsonSerializerSettings serializerSettings = serializerSettingsProvider?.Instance ?? JsonConvert.DefaultSettings?.Invoke();
            this.Properties = new PropertyBag(serializerSettings);
        }

        /// <inheritdoc/>
        public PartitionKeyDefinition PartitionKeyDefinition { get; set; }

        /// <inheritdoc/>
        public IndexingPolicy IndexingPolicy { get; set; }

        /// <inheritdoc/>
        public UniqueKeyPolicy UniqueKeyPolicy { get; set; }

        /// <inheritdoc/>
        public ConnectionPolicy ConnectionPolicy { get; set; }

        /// <inheritdoc/>
        public ConsistencyLevel? DesiredConsistencyLevel { get; set; }

        /// <inheritdoc/>
        public bool SupportKeyRotation { get; set; } = true;

        /// <inheritdoc/>
        public int? DefaultTimeToLive { get; set; }

        /// <inheritdoc/>
        public int DefaultOfferThroughput { get; set; } = 400;

        /// <inheritdoc/>
        public bool UseDatabaseThroughput { get; set; }

        /// <inheritdoc/>
        public bool DefaultOfferEnableRUPerMinuteThroughput { get; set; }

        /// <inheritdoc/>
        public Uri AccountUri { get; set; }

        /// <inheritdoc/>
        public string Database { get; set; }

        /// <inheritdoc/>
        public string Container { get; set; }

        /// <inheritdoc/>
        public PropertyBag Properties
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public bool TryGetProperty<T>(string key, out T result)
        {
            return this.Properties.TryGet<T>(key, out result);
        }

        /// <inheritdoc/>
        public void SetProperty<T>(string key, T value)
        {
            this.Properties.Set(key, value);
        }
    }
}
