// <copyright file="CosmosDbSetup.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Testing.CosmosDb.Extensions
{
    using System.Collections.Generic;
    using Corvus.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides common handling of Cosmos Client test setup.
    /// </summary>
    public static class CosmosDbSetup
    {
        /// <summary>
        /// Adds DI services required by tests that use real Cosmos DB collections (either in Azure
        /// or in the local emulator).
        /// </summary>
        /// <param name="serviceCollection">The service collection to modify.</param>
        /// <param name="partitionKeyPath">The partition key path to specify when creating the collection.</param>
        public static void AddSharedThroughputCosmosDbTestServices(
            this IServiceCollection serviceCollection,
            string partitionKeyPath)
        {
            AddStandardServices(serviceCollection);

            AddConfiguration(
                serviceCollection,
                "endjinspecssharedthroughput",
                "endjinspecsgraphsharedthroughput",
                partitionKeyPath);
        }

        /// <summary>
        /// Adds DI services required by tests that use real Cosmos DB collections (either in Azure
        /// or in the local emulator).
        /// </summary>
        /// <param name="serviceCollection">The service collection to modify.</param>
        /// <param name="sqlDatabaseName">The name of the CosmosDB database in which to create document collections.</param>
        /// <param name="graphDatabaseName">The name of the CosmosDB database in which to create graph collections.</param>
        public static void AddNonSharedThroughputCosmosDbTestServices(
            this IServiceCollection serviceCollection,
            string? sqlDatabaseName = null,
            string? graphDatabaseName = null)
        {
            AddStandardServices(serviceCollection);

            AddConfiguration(
                serviceCollection,
                sqlDatabaseName,
                graphDatabaseName,
                null);
        }

        private static void AddStandardServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTestNameProvider();
            serviceCollection.AddCosmosClientBuilder();
        }

        private static IConfigurationRoot AddConfiguration(
            IServiceCollection serviceCollection,
            string? cosmosDbDatabaseName,
            string? cosmosDbGraphDatabaseName,
            string? partitionKeyPath)
        {
            const string CosmosDbLocalEmulatorWellKnownKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            // At the base of the configuration build we have an in-memory provider that sets us
            // up to run with the local CosmosDB Emulator. This means that developers running tests
            // locally don't need to do anything besides installing and starting the emulator.
            // There's no need to populate any local configuration files, and there's no need
            // for the 'az' CLI to be logged in with suitable credentials.
            // Developers can supply a local.settings to specify a real CosmosDB instance for testing
            // if necessary. This is not a requirement - if this isn't here, we'll just fall back to
            // the emulator. However, this may be useful if it turns out to be necessary to debug
            // against a real CosmosDB instance up in Azure to repro a problem.
            // Typically this will set the CosmosDbAccountUri and CosmosDbKeySecretName. Generally
            // the other settings can remain the same.
            var fallbackSettings = new Dictionary<string, string>
                {
                    { "CosmosDbAccountUri", "https://localhost:8081/" },
                    { "CosmosDbKeySecretName", "SecretForLocalEmulator" },
                    { "Graph:CosmosDbAccountUri", "https://localhost:8081/" },
                    { "Graph:CosmosDbKeySecretName", "SecretForLocalEmulator" },
                    { "Graph:CosmosDbDefaultOfferThroughput", "400" },

                    // Note: the Cosmos DB emulator does not enable Gremlin by default. You can
                    // turn it on by adding this environment variable to your system:
                    //  AZURE_COSMOS_EMULATOR_GREMLIN_ENDPOINT=true
                    // Or you can run the emulator manually with the /EnableGremlinEndpoint switch
                    // See https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator for more details.
                    { "Graph:CosmosDbGremlinHost", "localhost" },
                    { "Graph:CosmosDbGremlinPort", "8901" },
                    { "kv:SecretForLocalEmulator", CosmosDbLocalEmulatorWellKnownKey },
                    { "CosmosDbDefaultOfferThroughput", "400" },
                };

            if (partitionKeyPath != null)
            {
                fallbackSettings.Add("CosmosDbPartitionKeyPath", partitionKeyPath);
                fallbackSettings.Add("CosmosDbUseDatabaseThroughput", "true");
            }

            if (cosmosDbGraphDatabaseName != null)
            {
                fallbackSettings.Add("Graph:CosmosDbDatabaseName", cosmosDbGraphDatabaseName);
            }

            if (cosmosDbDatabaseName != null)
            {
                fallbackSettings.Add("CosmosDbDatabaseName", cosmosDbDatabaseName);
            }

            var builder = new ConfigurationBuilder();
            builder.AddConfigurationForTest("local.settings.json", fallbackSettings);
            IConfigurationRoot configuration = builder.Build();
            serviceCollection.AddSingleton(configuration);
            return configuration;
        }
    }
}