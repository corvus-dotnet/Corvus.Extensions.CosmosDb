// <copyright file="CosmosDbContextBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Extensions;
    using Corvus.Extensions.Cosmos;
    using Corvus.Specflow.Extensions;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Services to help manage Cosmos DB instances in a Scenario or Feature context.
    /// </summary>
    [Binding]
    public static class CosmosDbContextBindings
    {
        private const string CosmosDbDatabasesToDelete = "CosmosDbDatabasesToDelete";
        private const string CosmosDbContainersToDelete = "CosmosDbContainersToDelete";

        /// <summary>
        /// Adds a <see cref="Container"/> to the list to be cleaned up once all scenarios
        /// for the feature have been tested.
        /// </summary>
        /// <param name="featureContext">The current feature context.</param>
        /// <param name="container">The <see cref="Container"/> to add to the list for clean-up.</param>
        /// <param name="database">The database to delete (or null if you do not wish to clean up the database for the container).</param>
        public static void AddFeatureLevelCosmosDbContainerForCleanup(
            FeatureContext featureContext,
            Container container,
            Database database = null)
        {
            AddCosmosDbContainerForCleanup(featureContext, container, database);
        }

        /// <summary>
        /// Adds a <see cref="Container"/> to the list to be cleaned up when the current
        /// scenario finishes.
        /// </summary>
        /// <param name="scenarioContext">The current scenario context.</param>
        /// <param name="container">The <see cref="Container"/> to add to the list for clean-up.</param>
        /// <param name="database">The database to delete (or null if you do not wish to clean up the database for the container).</param>
        public static void AddScenarioLevelCosmosDbContainerForCleanup(
            ScenarioContext scenarioContext,
            Container container,
            Database database = null)
        {
            AddCosmosDbContainerForCleanup(scenarioContext, container, database);
        }

        /// <summary>
        /// Set up Cosmos DB account details for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [BeforeFeature("@setupCosmosDBKeys", Order = CosmosDbBeforeFeatureOrder.ReadClientSettings)]
        [BeforeFeature("@setupCosmosDbContainer", Order = CosmosDbBeforeFeatureOrder.ReadClientSettings)]
        public static async Task SetupCosmosDbAccountKeys(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            IConfigurationRoot configRoot = serviceProvider.GetRequiredService<IConfigurationRoot>();

            CosmosDbSettings settings = configRoot.Get<CosmosDbSettings>();
            string keyVaultName = configRoot["KeyVaultName"];

            string secret = await SecretHelper.GetSecretFromConfigurationOrKeyVaultAsync(
                configRoot,
                "kv:" + settings.CosmosDbKeySecretName,
                keyVaultName,
                settings.CosmosDbKeySecretName);

            string partitionKeyPath = configRoot["CosmosDbPartitionKeyPath"];
            featureContext.Set(partitionKeyPath, CosmosDbContextKeys.PartitionKeyPath);
            featureContext.Set(settings);
            featureContext.Set(secret, CosmosDbContextKeys.AccountKey);
        }

        /// <summary>
        /// Enable setup of a Cosmos DB Sql Client for the feature using the
        /// <c>@setupCosmosDbContainer</c> tag.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.
        /// </para>
        /// <para>
        /// This method creates a unique container for the feature in a database configured for shared throughput (which will typically be shared amongst multiple features.)
        /// </para>
        /// <para>
        /// You will find a <see cref="CosmosDbContextKeys.CosmosDbClient"/>, <see cref="CosmosDbContextKeys.CosmosDbDatabase"/> and <see cref="CosmosDbContextKeys.CosmosDbContainer"/>
        /// stored in the FeatureContext.
        /// </para>
        /// <para>
        /// If there is an <see cref="ICosmosClientBuilderFactory"/> registered in the container it will use it, otherwise it will use a default client builder.
        /// </para>
        /// <para>
        /// It registers the container for deletion, but not the database, as this is expected to be a shared testing resource.
        /// </para>
        /// <para>
        /// You should use this in conjunction with the <c>@setupCosmosDbContainer</c> tag.
        /// </para>
        /// </remarks>
        [BeforeFeature("@withSharedDatabase", Order = CosmosDbBeforeFeatureOrder.CreateDatabase)]
        [BeforeFeature("@withUniqueFeatureContainerInSharedDatabase", Order = CosmosDbBeforeFeatureOrder.CreateDatabase)]
        public static async Task SetupCosmosDbDatabaseForFeature(FeatureContext featureContext)
        {
            CosmosDbSettings settings = featureContext.Get<CosmosDbSettings>();

            ICosmosClientBuilderFactory clientBuilderFactory = ContainerBindings.GetServiceProvider(featureContext).GetService<ICosmosClientBuilderFactory>();

            string accountKey = featureContext.Get<string>(CosmosDbContextKeys.AccountKey);

            CosmosClientBuilder builder;

            if (clientBuilderFactory is null)
            {
                builder = new CosmosClientBuilder(settings.CosmosDbAccountUri, accountKey);
            }
            else
            {
                builder = clientBuilderFactory.CreateCosmosClientBuilder(settings.CosmosDbAccountUri, accountKey);
            }

            builder = builder
                .WithConnectionModeDirect();

            CosmosClient client = builder.Build();
            Database database = await client.CreateDatabaseIfNotExistsAsync(settings.CosmosDbDatabaseName, settings.CosmosDbDefaultOfferThroughput);
            featureContext.Set(client, CosmosDbContextKeys.CosmosDbClient);
            featureContext.Set(database, CosmosDbContextKeys.CosmosDbDatabase);
        }

        /// <summary>
        /// Enable setup of a Cosmos DB Sql Client for the feature using the
        /// <c>@setupCosmosDbContainer</c> tag.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.
        /// </para>
        /// <para>
        /// This method creates a unique container for the feature in a database configured for shared throughput (which will typically be shared amongst multiple features.)
        /// </para>
        /// <para>
        /// You will find a <see cref="CosmosDbContextKeys.CosmosDbClient"/>, <see cref="CosmosDbContextKeys.CosmosDbDatabase"/> and <see cref="CosmosDbContextKeys.CosmosDbContainer"/>
        /// stored in the FeatureContext.
        /// </para>
        /// <para>
        /// If there is an <see cref="ICosmosClientBuilderFactory"/> registered in the container it will use it, otherwise it will use a default client builder.
        /// </para>
        /// <para>
        /// It registers the container for deletion, but not the database, as this is expected to be a shared testing resource.
        /// </para>
        /// <para>
        /// You should use this in conjunction with the <c>@setupCosmosDbContainer</c> tag.
        /// </para>
        /// </remarks>
        [BeforeFeature("@withUniqueFeatureContainerInSharedDatabase", Order = CosmosDbBeforeFeatureOrder.CreateContainer)]
        public static async Task SetupCosmosDbContainerForFeature(FeatureContext featureContext)
        {
            string partitionKeyPath = featureContext.Get<string>(CosmosDbContextKeys.PartitionKeyPath);
            Database database = featureContext.Get<Database>(CosmosDbContextKeys.CosmosDbDatabase);
            Container container = await database.CreateContainerIfNotExistsAsync("client-" + Guid.NewGuid(), partitionKeyPath);
            featureContext.Set(container, CosmosDbContextKeys.CosmosDbContainer);
            AddFeatureLevelCosmosDbContainerForCleanup(featureContext, container);
        }

        /// <summary>
        /// Temp.
        /// </summary>
        /// <param name="scenarioContext">The scenario context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupCosmosDBKeys", Order = 100000)]
        [AfterScenario("@withSharedDatabase", Order = 100000)]
        public static Task TeardownScenarioLevelCosmosDBContainers(ScenarioContext scenarioContext)
        {
            return scenarioContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBContainersCoreAsync(scenarioContext));
        }

        /// <summary>
        /// Tear down the cosmos DB client for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupCosmosDbContainer", Order = 100000)]
        [AfterFeature("@setupCosmosDBKeys", Order = 100000)]
        public static Task TeardownFeatureLevelCosmosDBContainer(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBContainersCoreAsync(featureContext));
        }

        /// <summary>
        /// Tear down the cosmos DB datbase for the sceanario.
        /// </summary>
        /// <param name="scenarioContext">The scenario context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupCosmosDbContainer", Order = 100001)]
        [AfterScenario("@setupCosmosDBKeys", Order = 100001)]
        public static Task TeardownScenarioLevelCosmosDBDatabases(ScenarioContext scenarioContext)
        {
            return scenarioContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBDatabasesCoreAsync(scenarioContext));
        }

        /// <summary>
        /// Tear down the cosmos DB database for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupCosmosDbContainer", Order = 100001)]
        [AfterFeature("@setupCosmosDBKeys", Order = 100001)]
        public static Task TeardownFeatureLevelCosmosDBDatabases(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBDatabasesCoreAsync(featureContext));
        }

        private static async Task TeardownCosmosDBContainersCoreAsync(SpecFlowContext context)
        {
            if (context.ContainsKey(CosmosDbContainersToDelete))
            {
                // We run this way not so much because we want to delete everything at once,
                // but because this way we do actually attempt every deletion, and then report
                // any failures at the end in one big AggregateException.
                await Task.WhenAll(context.Get<List<Container>>(CosmosDbContainersToDelete)
                    .Select(async container =>
                    {
                        try
                        {
                            await container.DeleteContainerAsync().ConfigureAwait(false);
                        }
                        catch (InvalidOperationException x)
                        when (x.Message == "Unable to use database throughput in a database that has already been created.")
                        {
                            // Swallow this because it's an expected exception in certain test scenarios.
                        }
                    }))
                    .ConfigureAwait(false);
            }
        }

        private static async Task TeardownCosmosDBDatabasesCoreAsync(
            SpecFlowContext context)
        {
            if (context.ContainsKey(CosmosDbDatabasesToDelete))
            {
                await Task.WhenAll(context.Get<List<Database>>(CosmosDbDatabasesToDelete)
                    .DistinctBy(database => database.Id)
                    .Select(async database => await database.DeleteAsync().ConfigureAwait(false)))
                    .ConfigureAwait(false);
            }
        }

        private static void AddCosmosDbContainerForCleanup(
            SpecFlowContext context,
            Container container,
            Database database = null)
        {
            if (!context.ContainsKey(CosmosDbContainersToDelete))
            {
                context.Set(new List<Container>(), CosmosDbContainersToDelete);
            }

            if (!(database is null))
            {
                if (!context.ContainsKey(CosmosDbDatabasesToDelete))
                {
                    context.Set(new List<Database>(), CosmosDbDatabasesToDelete);
                }

                List<Database> databasesToDelete = context.Get<List<Database>>(CosmosDbDatabasesToDelete);
                databasesToDelete.Add(database);
            }

            List<Container> containers = context.Get<List<Container>>(CosmosDbContainersToDelete);
            containers.Add(container);
        }
    }
}
