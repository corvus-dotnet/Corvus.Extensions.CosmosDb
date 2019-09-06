// <copyright file="CosmosClientBuilderFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos.Internal
{
    using Corvus.Extensions.Json;
    using Microsoft.Azure.Cosmos.Fluent;

    /// <summary>
    /// A factory which can create a <see cref="CosmosClientBuilder"/>.
    /// </summary>
    public class CosmosClientBuilderFactory : ICosmosClientBuilderFactory
    {
        private readonly IJsonSerializerSettingsProvider serializerSettingsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosClientBuilderFactory"/> class.
        /// </summary>
        /// <param name="serializerSettingsProvider">The <see cref="IJsonSerializerSettingsProvider"/> for the environment.</param>
        public CosmosClientBuilderFactory(IJsonSerializerSettingsProvider serializerSettingsProvider)
        {
            this.serializerSettingsProvider = serializerSettingsProvider ?? throw new System.ArgumentNullException(nameof(serializerSettingsProvider));
        }

        /// <inheritdoc />
        public CosmosClientBuilder CreateCosmosClientBuilder(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new System.ArgumentException("message", nameof(connectionString));
            }

            return new CosmosClientBuilder(connectionString).WithCustomSerializer(new CorvusJsonDotNetCosmosSerializer(this.serializerSettingsProvider.Instance));
        }

        /// <inheritdoc />
        public CosmosClientBuilder CreateCosmosClientBuilder(string accountEndpoint, string accountKey)
        {
            if (string.IsNullOrEmpty(accountEndpoint))
            {
                throw new System.ArgumentException("message", nameof(accountEndpoint));
            }

            if (string.IsNullOrEmpty(accountKey))
            {
                throw new System.ArgumentException("message", nameof(accountKey));
            }

            return new CosmosClientBuilder(accountEndpoint, accountKey).WithCustomSerializer(new CorvusJsonDotNetCosmosSerializer(this.serializerSettingsProvider.Instance));
        }
    }
}
