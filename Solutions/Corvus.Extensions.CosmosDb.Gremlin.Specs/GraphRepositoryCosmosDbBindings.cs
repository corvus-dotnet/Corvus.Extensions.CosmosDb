// <copyright file="GraphRepositoryCosmosDbBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.GraphRepository.Specs
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb;
    using Corvus.Extensions.CosmosDb.Crypto;
    using Corvus.Extensions.Json;
    using Corvus.SpecFlow.Extensions;
    using Corvus.SpecFlow.Extensions.CosmosDb;
    using Gremlin.Net.Driver;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support Cosmos DB.
    /// </summary>
    [Binding]
    public static class GraphRepositoryCosmosDbBindings
    {
        /// <summary>
        /// Set up a Cosmos DB Graph Client for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        /// <returns>A task that completes when the method completes.</returns>
        [BeforeFeature("@setupGraphRepository", Order = CosmosDbBeforeFeatureOrder.CreateClient)]
        public static async Task SetupGraphRepository(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            IConfigurationRoot configRoot = serviceProvider.GetRequiredService<IConfigurationRoot>();

            string keyVaultName = configRoot["KeyVaultName"];

            IConfiguration config = configRoot.GetSection("Graph");
            CosmosDbGremlinSettings settings = config.Get<CosmosDbGremlinSettings>();
            bool useSslForGremlin = settings.CosmosDbGremlinHost != "localhost";

            string secret = await SecretHelper.GetSecretFromConfigurationOrKeyVaultAsync(
                configRoot,
                "kv:" + settings.CosmosDbKeySecretName,
                keyVaultName,
                settings.CosmosDbKeySecretName);

            var partitionKeyDefinition = new PartitionKeyDefinition();
            partitionKeyDefinition.Paths.Add("/partition");
            var docRepo = new CosmosDbSqlClient(
                settings.CosmosDbDatabaseName,
                "client-graph-" + Guid.NewGuid(),
                settings.CosmosDbAccountUri,
                secret,
                ContainerBindings.GetServiceProvider(featureContext).GetService<IJsonSerializerSettingsProvider>().Instance,
                partitionKeyDefinition,
                connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                defaultOfferThroughput: settings.CosmosDbDefaultOfferThroughput,
                useDatabaseThroughput: true);

            var client = new CosmosDbGremlinClient(docRepo, settings.CosmosDbGremlinHost, settings.CosmosDbGremlinPort, useSslForGremlin);
            featureContext.Set(client, CosmosDbContextKeys.CosmosDbClient);
        }

        /// <summary>
        /// Tear down the cosmos DB client for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupGraphRepository", Order = 100000)]
        public static async Task TearDownGraphRepository(FeatureContext featureContext)
        {
            try
            {
                ICosmosDbGremlinClient client = featureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
                await client.DeleteCollectionAsync().ConfigureAwait(false);
                client.Dispose();
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // NOP
            }
        }
    }
}
