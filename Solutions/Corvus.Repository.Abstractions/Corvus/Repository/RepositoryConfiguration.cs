// <copyright file="RepositoryConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Corvus.ContentHandling.Json;
    using Corvus.Extensions;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Encapsulates a complete repository configuration.
    /// </summary>
    public class RepositoryConfiguration
    {
        private IDictionary<string, string> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryConfiguration"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use in the context.</param>
        public RepositoryConfiguration(IServiceProvider serviceProvider)
        {
            IDefaultJsonSerializerSettings serializerSettings = serviceProvider.GetService<IDefaultJsonSerializerSettings>();
            this.SerializerSettings = serializerSettings?.Instance ?? CreateJsonSerializerSettings(serviceProvider);
        }

        /// <summary>
        /// Gets or sets the partition key definition.
        /// </summary>
        public PartitionKeyDefinition PartitionKeyDefinition { get; set; }

        /// <summary>
        /// Gets or sets the indexing policy.
        /// </summary>
        public IndexingPolicy IndexingPolicy { get; set; }

        /// <summary>
        /// Gets or sets the unique key policy.
        /// </summary>
        public UniqueKeyPolicy UniqueKeyPolicy { get; set; }

        /// <summary>
        /// Gets or sets the connection policy.
        /// </summary>
        public ConnectionPolicy ConnectionPolicy { get; set; }

        /// <summary>
        /// Gets or sets the desired consistency level.
        /// </summary>
        public ConsistencyLevel? DesiredConsistencyLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether key rotation is supported.
        /// </summary>
        public bool SupportKeyRotation { get; set; } = true;

        /// <summary>
        /// Gets or sets the default time to live for a document.
        /// </summary>
        public int? DefaultTimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the default offer for throughput.
        /// </summary>
        public int DefaultOfferThroughput { get; set; } = 400;

        /// <summary>
        /// Gets or sets a value indicating whether to use database-level throughput.
        /// </summary>
        public bool UseDatabaseThroughput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RU Per Minute Throughput is offered by default.
        /// </summary>
        public bool DefaultOfferEnableRUPerMinuteThroughput { get; set; }

        /// <summary>
        /// Gets or sets the account URI.
        /// </summary>
        public Uri AccountUri { get; set; }

        /// <summary>
        /// Gets or sets the database name. If set, this overrides the name specified in
        /// <see cref="RepositoryDefinition.Database"/>.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified in
        /// <see cref="RepositoryDefinition.Container"/>.
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Container"/> property should be
        /// used verbatim, or whether it should be used in conjunction with a tenant ID prefix.
        /// </summary>
        public bool DisableContainerTenantIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerSettings"/> to use when serializing the extension properties.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets the collection of properties for this configuration.
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get
            {
                return this.properties ?? (this.properties = new Dictionary<string, string>());
            }

            set
            {
                this.properties = value;
            }
        }

        /// <summary>
        /// Get a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the object was found.</returns>
        public bool TryGetProperty<T>(string key, out T result)
        {
            if (this.Properties.TryGetValue(key, out string stringResult))
            {
                result = JsonConvert.DeserializeObject<T>(stringResult, this.SerializerSettings);
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Set a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key for the property.</param>
        /// <param name="value">The value of the property.</param>
        public void SetProperty<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value, this.SerializerSettings);
            this.Properties.ReplaceIfExists(key, serializedValue);
        }

        private static JsonSerializerSettings CreateJsonSerializerSettings(IServiceProvider serviceProvider)
        {
            // Otherwise fall back on compatible defaults.
            return new JsonSerializerSettings
            {
                ContractResolver = StandardContractResolver.Instance,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ConstructorHandling = ConstructorHandling.Default,
                TypeNameHandling = TypeNameHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                FloatParseHandling = FloatParseHandling.Double,
                FloatFormatHandling = FloatFormatHandling.String,
                StringEscapeHandling = StringEscapeHandling.Default,
                CheckAdditionalContent = false,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK",
                Converters = new List<JsonConverter>(serviceProvider.GetServices<JsonConverter>()),
                ReferenceResolverProvider = null,
                Context = default,
                Culture = CultureInfo.InvariantCulture,
                MaxDepth = 4096,
                DefaultValueHandling = DefaultValueHandling.Include,
            };
        }

        private class StandardContractResolver : CamelCasePropertyNamesContractResolver
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StandardContractResolver"/> class.
            /// </summary>
            public StandardContractResolver()
            {
                this.NamingStrategy.ProcessDictionaryKeys = false;
            }

            /// <summary>
            /// Gets a standard endjin contract resolver.
            /// </summary>
            public static StandardContractResolver Instance { get; } = new StandardContractResolver();
        }
    }
}
