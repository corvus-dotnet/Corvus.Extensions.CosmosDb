// <copyright file="RepositoryCosmosDbBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Repository.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.ContentHandling.Json;
    using Corvus.Extensions;
    using Corvus.SpecFlow.Extensions;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support Cosmos DB.
    /// </summary>
    [Binding]
    public static class RepositoryCosmosDbBindings
    {
        private const string CosmosDbRepositories = "CosmosDbRepositories";

        private const string CosmosDbsToDelete = "CosmosDbsToDelete";

        /// <summary>
        /// Set up Cosmos DB account details for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [BeforeFeature("@setupCosmosDBKeys", Order = CosmosDbBeforeFeatureOrder.ReadRepositorySettings)]
        [BeforeFeature("@setupCosmosDBRepository", Order = CosmosDbBeforeFeatureOrder.ReadRepositorySettings)]
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

            featureContext.Set(settings);
            featureContext.Set(secret, CosmosDbContextKeys.AccountKey);
        }

        /// <summary>
        /// Adds a <see cref="DocumentRepository"/> to the list to be cleaned up once all scenarios
        /// for the feature have been tested.
        /// </summary>
        /// <param name="featureContext">The current feature context.</param>
        /// <param name="repository">The <see cref="DocumentRepository"/> to add to the list for clean-up.</param>
        /// <param name="deleteDb">Delete the containing db.</param>
        public static void AddFeatureLevelCosmosDbRepositoryForCleanup(
            FeatureContext featureContext,
            DocumentRepository repository,
            bool deleteDb = false)
        {
            AddCosmosDbRepositoryForCleanup(featureContext, repository, deleteDb);
        }

        /// <summary>
        /// Adds a <see cref="DocumentRepository"/> to the list to be cleaned up when the current
        /// scenario finishes.
        /// </summary>
        /// <param name="scenarioContext">The current feature context.</param>
        /// <param name="repository">The <see cref="DocumentRepository"/> to add to the list for clean-up.</param>
        /// <param name="deleteDb">Delete the containing db.</param>
        public static void AddScenarioLevelCosmosDbRepositoryForCleanup(
            ScenarioContext scenarioContext,
            DocumentRepository repository,
            bool deleteDb = false)
        {
            AddCosmosDbRepositoryForCleanup(scenarioContext, repository, deleteDb);
        }

        /// <summary>
        /// Enable setup of a Cosmos DB Repository for the feature using the
        /// <c>@setupCosmosDBRepository</c> tag.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeFeature("@setupCosmosDBRepository", Order = CosmosDbBeforeFeatureOrder.CreateRepository)]
        public static void SetupCosmosDbRepositoryForFeature(FeatureContext featureContext)
        {
            CosmosDbSettings settings = featureContext.Get<CosmosDbSettings>();

            IDefaultJsonSerializerSettings serializerSettings = ContainerBindings.GetServiceProvider(featureContext).GetRequiredService<IDefaultJsonSerializerSettings>();

            var partitionKeyDefinition = new PartitionKeyDefinition();
            partitionKeyDefinition.Paths.Add("/id");
            var repository = new DocumentRepository(
                settings.CosmosDbDatabaseName,
                "repo-" + Guid.NewGuid(),
                settings.CosmosDbAccountUri,
                featureContext.Get<string>(CosmosDbContextKeys.AccountKey),
                serializerSettings.Instance,
                partitionKeyDefinition,
                connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                defaultOfferThroughput: settings.CosmosDbDefaultOfferThroughput,
                useDatabaseThroughput: true);
            featureContext.Set(repository, CosmosDbContextKeys.CosmosDbRepository);
            AddFeatureLevelCosmosDbRepositoryForCleanup(featureContext, repository);
        }

        /// <summary>
        /// Temp.
        /// </summary>
        /// <param name="scenarioContext">The scenario context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupCosmosDBKeys", Order = 100000)]
        public static Task TeardownScenarioLevelCosmosDBRepositories(ScenarioContext scenarioContext)
        {
            return scenarioContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBRepositories(scenarioContext));
        }

        /// <summary>
        /// Tear down the cosmos DB repository for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupCosmosDBRepository", Order = 100000)]
        [AfterFeature("@setupCosmosDBKeys", Order = 100000)]
        public static Task TeardownFeatureLevelCosmosDBRepositories(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBRepositories(featureContext));
        }

        /// <summary>
        /// Tear down the cosmos DB repository for the sceanario.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <param name="scenarioContext">The scenario context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupCosmosDBRepository", Order = 100001)]
        [AfterScenario("@setupCosmosDBKeys", Order = 100001)]
        public static Task TeardownScenarioLevelCosmosDBDatabases(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            CosmosDbSettings settings = featureContext.Get<CosmosDbSettings>();
            string accountKey = featureContext.Get<string>(CosmosDbContextKeys.AccountKey);
            return scenarioContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBDatabasesCore(settings, accountKey, scenarioContext));
        }

        /// <summary>
        /// Tear down the cosmos DB repository for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupCosmosDBRepository", Order = 100001)]
        [AfterFeature("@setupCosmosDBKeys", Order = 100001)]
        public static Task TeardownFeatureLevelCosmosDBDatabases(FeatureContext featureContext)
        {
            CosmosDbSettings settings = featureContext.Get<CosmosDbSettings>();
            string accountKey = featureContext.Get<string>(CosmosDbContextKeys.AccountKey);
            return featureContext.RunAndStoreExceptionsAsync(() => TeardownCosmosDBDatabasesCore(settings, accountKey, featureContext));
        }

        private static DocumentRepository SetupCosmosDbRepositoryCore(SpecFlowContext context)
        {
            CosmosDbSettings settings = context.Get<CosmosDbSettings>();

            IDefaultJsonSerializerSettings serializerSettings = ContainerBindings.GetServiceProvider(featureContext).GetRequiredService<IDefaultJsonSerializerSettings>();


            var partitionKeyDefinition = new PartitionKeyDefinition();
            partitionKeyDefinition.Paths.Add("/id");
            var repository = new DocumentRepository(
                settings.CosmosDbDatabaseName,
                "repo-" + Guid.NewGuid(),
                settings.CosmosDbAccountUri,
                context.Get<string>(CosmosDbContextKeys.AccountKey),
                serializerSettings.Instance,
                partitionKeyDefinition,
                connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                defaultOfferThroughput: settings.CosmosDbDefaultOfferThroughput,
                useDatabaseThroughput: true);
            context.Set(repository, CosmosDbContextKeys.CosmosDbRepository);
            return repository;
        }

        private static void AddCosmosDbRepositoryForCleanup(
            SpecFlowContext context,
            DocumentRepository repository,
            bool deleteDb)
        {
            if (!context.ContainsKey(CosmosDbRepositories))
            {
                context.Set(new List<DocumentRepository>(), CosmosDbRepositories);
            }

            if (deleteDb)
            {
                if (!context.ContainsKey(CosmosDbsToDelete))
                {
                    context.Set(new List<DocumentRepository>(), CosmosDbsToDelete);
                }

                List<DocumentRepository> repositoriesToDeleteDbsFor = context.Get<List<DocumentRepository>>(CosmosDbsToDelete);
                repositoriesToDeleteDbsFor.Add(repository);
            }

            List<DocumentRepository> repositories = context.Get<List<DocumentRepository>>(CosmosDbRepositories);
            repositories.Add(repository);
        }

        private static async Task TeardownCosmosDBRepositories(SpecFlowContext context)
        {
            if (context.ContainsKey(CosmosDbRepositories))
            {
                // We run this way not so much because we want to delete everything at once,
                // but because this way we do actually attempt every deletion, and then report
                // any failures at the end in one big AggregateException.
                await Task.WhenAll(context.Get<List<DocumentRepository>>(CosmosDbRepositories)
                    .Select(async item =>
                    {
                        try
                        {
                            await item.DeleteRepositoryContainerAsync();
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

        private static async Task TeardownCosmosDBDatabasesCore(
            CosmosDbSettings settings,
            string accountKey,
            SpecFlowContext context)
        {
            if (context.ContainsKey(CosmosDbsToDelete))
            {
                await Task.WhenAll(context.Get<List<DocumentRepository>>(CosmosDbsToDelete)
                    .DistinctBy(item => item.Database)
                    .Select(async item =>
                    {
                        var documentClient = new DocumentClient(settings.CosmosDbAccountUri, accountKey, item.DefaultConnectionPolicy, item.DefaultDesiredConsistencyLevel);
                        await documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(item.Database)).ConfigureAwait(false);
                    }))
                    .ConfigureAwait(false);
            }
        }
    }
}
