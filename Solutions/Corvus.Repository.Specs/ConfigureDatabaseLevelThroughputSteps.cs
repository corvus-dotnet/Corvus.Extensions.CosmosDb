// <copyright file="ConfigureDatabaseLevelThroughputSteps.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

#pragma warning disable CS1591, SA1600 // Elements should be documented

namespace Corvus.Repository.Specs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.SpecFlow.CosmosDb;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Bindings for configuring database-level throughput specs.
    /// </summary>
    [Binding]
    public class ConfigureDatabaseLevelThroughputSteps
    {
        private const string RunIdKey = "RunId";
        private readonly string runId = Guid.NewGuid().ToString();
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;

        public ConfigureDatabaseLevelThroughputSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
            this.scenarioContext.Set(this.runId, RunIdKey);
        }

        internal CosmosDbSettings DbSettings => this.featureContext.Get<CosmosDbSettings>();

        [When(@"I set the database offer throughput for the repository \(""(.*)"", ""(.*)""\) to (.*) ru/s")]
        public async Task WhenISetTheDatabaseOfferThroughputForTheRepositoryToRuS(string database, string collection, int throughput)
        {
            try
            {
                IDocumentRepository repository = this.scenarioContext.Get<IDocumentRepository>(GetRepositoryKey(database, collection));
                await repository.SetThroughputAsync(throughput).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When(@"I create a repository \(""(.*)"", ""(.*)""\) with database throughput (.*) RU/s")]
        [Given(@"I create a repository \(""(.*)"", ""(.*)""\) with database throughput (.*) RU/s")]
        public async Task WhenICreateARepositoryWithDatabaseThroughputRUS(string database, string collection, int throughput)
        {
            try
            {
                var partitionKeyDefinition = new PartitionKeyDefinition();
                partitionKeyDefinition.Paths.Add("/irrelevant");
                var repository = new DocumentRepository(
                    this.GetDatabaseName(database),
                    collection,
                    this.DbSettings.CosmosDbAccountUri,
                    this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey),
                    SerializerSettings.CreateSerializationSettings(),
                    partitionKeyDefinition,
                    connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                    defaultOfferThroughput: throughput,
                    useDatabaseThroughput: true);
                this.scenarioContext.Set(repository, GetRepositoryKey(database, collection));
                RepositoryCosmosDbBindings.AddScenarioLevelCosmosDbRepositoryForCleanup(this.scenarioContext, repository, deleteDb: true);
                await repository.GetDocumentClientAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given(@"I create a repository \(""(.*)"", ""(.*)""\) with collection throughput (.*) RU/s")]
        [When(@"I create a repository \(""(.*)"", ""(.*)""\) with collection throughput (.*) RU/s")]
        public async Task WhenICreateARepositoryWithCollectionThroughputRUS(string database, string collection, int throughput)
        {
            try
            {
                var partitionKeyDefinition = new PartitionKeyDefinition();
                partitionKeyDefinition.Paths.Add("/irrelevant");
                var repository = new DocumentRepository(
                    $"{database}__{this.scenarioContext.Get<string>(RunIdKey)}",
                    collection,
                    this.DbSettings.CosmosDbAccountUri,
                    this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey),
                    SerializerSettings.CreateSerializationSettings(),
                    partitionKeyDefinition,
                    connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                    defaultOfferThroughput: throughput,
                    useDatabaseThroughput: false);
                this.scenarioContext.Set(repository, GetRepositoryKey(database, collection));
                RepositoryCosmosDbBindings.AddScenarioLevelCosmosDbRepositoryForCleanup(this.scenarioContext, repository, deleteDb: true);
                await repository.GetDocumentClientAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Then(@"it should create a database called ""(.*)"" with the following collections")]
        public async Task ThenItShouldCreateADatabaseCalledWithTheFollowingCollections(string database, Table collections)
        {
            string databaseName = this.GetDatabaseName(database);
            var documentClient = new DocumentClient(this.DbSettings.CosmosDbAccountUri, this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey));
            foreach (TableRow collection in collections.Rows)
            {
                Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, collection["Collection name"]);
                ResourceResponse<DocumentCollection> documentCollection = await documentClient.ReadDocumentCollectionAsync(collectionLink).ConfigureAwait(false);
                int? throughput = await GetCollectionOfferThroughputAsync(documentClient, documentCollection).ConfigureAwait(false);
                if (throughput.HasValue)
                {
                    Assert.AreEqual(int.Parse(collection["Throughput"]), throughput.Value);
                }
                else
                {
                    Assert.AreEqual("NotSpecified", collection["Throughput"]);
                }
            }
        }

        [Then(@"the database called ""(.*)"" should have the throughput (.*) RU/s")]
        public async Task ThenTheDatabaseCalledShouldHaveTheThroughputRUS(string database, string throughput)
        {
            string databaseName = this.GetDatabaseName(database);
            var documentClient = new DocumentClient(this.DbSettings.CosmosDbAccountUri, this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey));
            Uri databaseLink = UriFactory.CreateDatabaseUri(databaseName);
            ResourceResponse<Database> databaseResource = await documentClient.ReadDatabaseAsync(databaseLink).ConfigureAwait(false);
            int? actualThroughput = await GetDatabaseOfferThroughputAsync(documentClient, databaseResource).ConfigureAwait(false);
            if (actualThroughput.HasValue)
            {
                Assert.AreEqual(int.Parse(throughput), actualThroughput.Value);
            }
            else
            {
                Assert.AreEqual("NotSpecified", throughput);
            }
        }

        private static string GetRepositoryKey(string database, string collection)
        {
            return $"{database}__{collection}";
        }

        private static async Task<int?> GetCollectionOfferThroughputAsync(DocumentClient documentClient, ResourceResponse<DocumentCollection> documentCollection)
        {
            IDocumentQuery<Offer> queryable = documentClient.CreateOfferQuery()
            .Where(r => r.ResourceLink == documentCollection.Resource.SelfLink)
            .AsDocumentQuery();

            FeedResponse<Offer> responseFeed = await queryable.ExecuteNextAsync<Offer>().ConfigureAwait(false);
            dynamic result = responseFeed.FirstOrDefault();
            return result?.Content.OfferThroughput;
        }

        private static async Task<int?> GetDatabaseOfferThroughputAsync(DocumentClient documentClient, ResourceResponse<Database> database)
        {
            IDocumentQuery<Offer> queryable = documentClient.CreateOfferQuery()
            .Where(r => r.ResourceLink == database.Resource.SelfLink)
            .AsDocumentQuery();

            FeedResponse<Offer> responseFeed = await queryable.ExecuteNextAsync<Offer>().ConfigureAwait(false);
            dynamic result = responseFeed.FirstOrDefault();
            return result?.Content.OfferThroughput;
        }

        private string GetDatabaseName(string database)
        {
            return $"{database}__{this.scenarioContext.Get<string>(RunIdKey)}";
        }
    }
}

#pragma warning restore CS1591, SA1600 // Elements should be documented
