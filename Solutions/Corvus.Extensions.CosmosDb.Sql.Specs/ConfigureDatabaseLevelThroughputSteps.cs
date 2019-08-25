// <copyright file="ConfigureDatabaseLevelThroughputSteps.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

#pragma warning disable CS1591, SA1600 // Elements should be documented

namespace Corvus.Extensions.CosmosDb.Specs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Extensions.Json;
    using Corvus.SpecFlow.Extensions;
    using Corvus.SpecFlow.Extensions.CosmosDb;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.DependencyInjection;
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

        [When(@"I set the database offer throughput for the client \(""(.*)"", ""(.*)""\) to (.*) ru/s")]
        public async Task WhenISetTheDatabaseOfferThroughputForTheClientToRuS(string database, string collection, int throughput)
        {
            try
            {
                ICosmosDbSqlClient client = this.scenarioContext.Get<ICosmosDbSqlClient>(GetCosmosDbSqlClientKey(database, collection));
                await client.SetThroughputAsync(throughput).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When(@"I create a database collection \(""(.*)"", ""(.*)""\) with database throughput (.*) RU/s")]
        [Given(@"I create a database collection \(""(.*)"", ""(.*)""\) with database throughput (.*) RU/s")]
        public async Task WhenICreateADatabaseCollectionWithDatabaseThroughputRUS(string database, string collection, int throughput)
        {
            try
            {
                var partitionKeyDefinition = new PartitionKeyDefinition();
                partitionKeyDefinition.Paths.Add("/irrelevant");
                var client = new CosmosDbSqlClient(
                    this.GetDatabaseName(database),
                    collection,
                    this.DbSettings.CosmosDbAccountUri,
                    this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey),
                    ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<IJsonSerializerSettingsProvider>().Instance,
                    partitionKeyDefinition,
                    connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                    defaultOfferThroughput: throughput,
                    useDatabaseThroughput: true);
                this.scenarioContext.Set(client, GetCosmosDbSqlClientKey(database, collection));
                CosmosDbSqlClientBindings.AddScenarioLevelCosmosDbClientForCleanup(this.scenarioContext, client, deleteDb: true);
                await client.GetDocumentClientAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given(@"I create a database collection \(""(.*)"", ""(.*)""\) with collection throughput (.*) RU/s")]
        [When(@"I create a database collection \(""(.*)"", ""(.*)""\) with collection throughput (.*) RU/s")]
        public async Task WhenICreateADatabaseCollectionWithCollectionThroughputRUS(string database, string collection, int throughput)
        {
            try
            {
                var partitionKeyDefinition = new PartitionKeyDefinition();
                partitionKeyDefinition.Paths.Add("/irrelevant");
                var client = new CosmosDbSqlClient(
                    $"{database}__{this.scenarioContext.Get<string>(RunIdKey)}",
                    collection,
                    this.DbSettings.CosmosDbAccountUri,
                    this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey),
                    ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<IJsonSerializerSettingsProvider>().Instance,
                    partitionKeyDefinition,
                    connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct },
                    defaultOfferThroughput: throughput,
                    useDatabaseThroughput: false);
                this.scenarioContext.Set(client, GetCosmosDbSqlClientKey(database, collection));
                CosmosDbSqlClientBindings.AddScenarioLevelCosmosDbClientForCleanup(this.scenarioContext, client, deleteDb: true);
                await client.GetDocumentClientAsync().ConfigureAwait(false);
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
            using (var documentClient = new DocumentClient(this.DbSettings.CosmosDbAccountUri, this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey)))
            {
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
        }

        [Then(@"the database called ""(.*)"" should have the throughput (.*) RU/s")]
        public async Task ThenTheDatabaseCalledShouldHaveTheThroughputRUS(string database, string throughput)
        {
            string databaseName = this.GetDatabaseName(database);
            using (var documentClient = new DocumentClient(this.DbSettings.CosmosDbAccountUri, this.featureContext.Get<string>(CosmosDbContextKeys.AccountKey)))
            {
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
        }

        private static string GetCosmosDbSqlClientKey(string database, string collection)
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
