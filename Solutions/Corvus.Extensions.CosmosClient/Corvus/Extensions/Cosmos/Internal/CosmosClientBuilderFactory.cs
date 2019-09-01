// <copyright file="CosmosClientBuilderFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos.Internal
{
    using System.Threading.Tasks;
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
            this.serializerSettingsProvider = serializerSettingsProvider;
        }

        /// <inheritdoc />
        public Task<CosmosClientBuilder> CreateCosmosClientBuilder(string connectionString)
        {
            return Task.FromResult(new CosmosClientBuilder(connectionString).WithCustomSerializer(new CorvusJsonDotNetCosmosSerializer(this.serializerSettingsProvider.Instance)));
        }

        /// <inheritdoc />
        public Task<CosmosClientBuilder> CreateCosmosClientBuilder(string accountEndpoint, string accountKey)
        {
            return Task.FromResult(new CosmosClientBuilder(accountEndpoint, accountKey).WithCustomSerializer(new CorvusJsonDotNetCosmosSerializer(this.serializerSettingsProvider.Instance)));
        }
    }
}
