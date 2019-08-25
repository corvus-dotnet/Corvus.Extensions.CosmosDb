// <copyright file="CosmosDbSqlClientSteps.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Elements should be documented

namespace Corvus.Extensions.CosmosDb.Specs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb;
    using Corvus.Extensions.CosmosDb.Crypto;
    using Corvus.SpecFlow.Extensions.CosmosDb;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using NUnit.Framework;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public class CosmosDbSqlClientSteps : CosmosDbSqlClientStepsBase<SampleEntity>
    {
        private const string NullString = "null";
        private const string NewGuidString = "newguid";

        private const string StorageResultsKey = "StorageResult";
        private const string StoredEntitiesKey = "StoredEntities";
        private const string QueryValueKey = "QueryValue";

        public CosmosDbSqlClientSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(featureContext, scenarioContext)
        {
        }

        [BeforeFeature("@setup10SampleEntities", Order = CosmosDbBeforeFeatureOrder.PopulateDatabaseCollection)]
        public static Task Setup10SampleEntities(FeatureContext featureContext)
        {
            return AddRandomEntities(featureContext, 10);
        }

        [Then("the document client should not be null")]
        public async Task ThenTheDocumentClientShouldNotBeNull()
        {
            ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
            Assert.IsNotNull(await client.GetDocumentClientAsync().ConfigureAwait(false));
        }

        [Given("I delete an entity with ID '(.*)'")]
        [When("I delete an entity with ID '(.*)'")]
        public async Task WhenIDeleteAnEntityWithID(string id)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);

                string translatedId = this.TranslateId(id);
                await client.DeleteAsync(translatedId, translatedId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given("I set the offer throughput to (.*) ru/s")]
        [When("I set the offer throughput to (.*) ru/s")]
        public async Task WhenISetTheOfferThroughputToRuS(int rus)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
                await client.SetThroughputAsync(rus).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Then("the result should be (.*) ru/s")]
        public async Task ThenTheResultShouldBeRuS(int rus)
        {
            ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
            int throughput = await client.GetThroughputAsync().ConfigureAwait(false);
            Assert.AreEqual(rus, throughput);
        }

        [Then("it should throw an '(.*)'")]
        [Then("it should throw a '(.*)'")]
        public void ThenItShouldThrowAn(string exceptionTypeName)
        {
            var exceptionType = Type.GetType(exceptionTypeName);
            if (this.ScenarioContext.TryGetValue(CosmosDbContextKeys.ExceptionKey, out Exception exception))
            {
                Assert.IsInstanceOf(exceptionType, exception);
            }
            else
            {
                Assert.True(false, "No exception thrown.");
            }
        }

        [Then("the DocumentClientException should have an HTTP status code of '(.*)'")]
        public void ThenTheDocumentClientExceptionShouldHaveAnHTTPStatusCodeOf(string statusCode)
        {
            if (this.ScenarioContext.TryGetValue(CosmosDbContextKeys.ExceptionKey, out Exception exception))
            {
                if (exception is DocumentClientException documentClientException)
                {
                    var httpStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCode, true);
                    Assert.AreEqual(httpStatusCode, documentClientException.StatusCode);
                }
                else
                {
                    Assert.True(false, "The exception was not a DocumentClientException. (Have you verified with \"It should throw a 'Microsoft.Azure.Documents.DocumentClientException'\"?)");
                }
            }
            else
            {
                Assert.True(false, "No exception thrown.");
            }
        }

        [Then("the ArgumentNullException applies to the parameter '(.*)'")]
        public void ThenTheArgumentNullExceptionAppliesToTheParameter(string paramName)
        {
            if (this.ScenarioContext.TryGetValue(CosmosDbContextKeys.ExceptionKey, out Exception exception))
            {
                if (exception is ArgumentNullException argumentNullException)
                {
                    Assert.AreEqual(paramName, argumentNullException.ParamName);
                }
                else
                {
                    Assert.True(false, "The exception was not a DocumentClientException. (Have you verified with \"It should throw a 'Microsoft.Azure.Documents.DocumentClientException'\"?)");
                }
            }
            else
            {
                Assert.True(false, "No exception thrown.");
            }
        }

        [When("I store a null entity")]
        public Task WhenIStoreANullEntity()
        {
            return this.StoreAnEntity(null);
        }

        [Given("I store an entity")]
        [When("I store an entity")]
        public Task WhenIStoreAnEntity(Table table)
        {
            SampleEntity entity = table.CreateInstance<SampleEntity>();
            return this.StoreAnEntity(entity);
        }

        [Given("I insert an entity")]
        [When("I insert an entity")]
        public Task WhenIInsertAnEntity(Table table)
        {
            SampleEntity entity = table.CreateInstance<SampleEntity>();
            return this.InsertAnEntity(entity);
        }

        [Given("I save the ETag as '(.*)'")]
        public void GivenISaveTheETagAs(string key)
        {
            ResourceResponse<Document> result = this.ScenarioContext.Get<ResourceResponse<Document>>(ResultKey);
            this.ScenarioContext.Set(result.Resource.ETag, key);
        }

        [When("I store an entity with the ETag '(.*)'")]
        public Task WhenIStoreAnEntityWithTheETag(string key, Table table)
        {
            SampleEntity entity = table.CreateInstance<SampleEntity>();
            return this.StoreAnEntity(entity, this.ScenarioContext.Get<string>(key));
        }

        [When("I update an entity with the ETag '(.*)'")]
        public Task WhenIUpdateAnEntityWithTheETag(string key, Table table)
        {
            SampleEntity entity = table.CreateInstance<SampleEntity>();
            return this.UpdateAnEntity(entity, this.ScenarioContext.Get<string>(key));
        }

        [Then("the result should match")]
        public void ThenTheResultShouldMatch(Table table)
        {
            SampleEntity expected = table.CreateInstance<SampleEntity>();
            ResourceResponse<Document> actualResponse = this.ScenarioContext.Get<ResourceResponse<Document>>(ResultKey);
            SampleEntity actual = actualResponse.Resource.As<SampleEntity>();
            IsMatch(expected, actual);
        }

        [Given("I store the entities")]
        public async Task GivenIStoreTheEntities(Table table)
        {
            ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);

            try
            {
                await table.CreateSet<SampleEntity>().ForEachFailEndAsync(async entity =>
                {
                    ResourceResponse<Document> storageResult = await client.UpsertAsync(entity).ConfigureAwait(false);
                    if (!this.ScenarioContext.TryGetValue<List<ResourceResponse<Document>>>(StorageResultsKey, out List<ResourceResponse<Document>> storageResults))
                    {
                        storageResults = new List<ResourceResponse<Document>>();
                        this.ScenarioContext.Set(storageResults, StorageResultsKey);
                    }

                    storageResults.Add(storageResult);
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When("I get an entity with ID '(.*)'")]
        public async Task WhenIGetAnEntityWithID(string id)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
                string translatedId = this.TranslateId(id);
                SampleEntity entity = await client.GetAsync<SampleEntity>(translatedId, translatedId).ConfigureAwait(false);
                this.ScenarioContext.Set(entity, ResultKey);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When("I read a document with ID '(.*)'")]
        public async Task WhenIReadADocumentWithID(string id)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
                string translatedId = this.TranslateId(id);
                DocumentResponse<SampleEntity> document = await client.ReadDocumentAsync<SampleEntity>(
                    translatedId, translatedId).ConfigureAwait(false);
                this.ScenarioContext.Set(document, ResultKey);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When("I get a document with ID '(.*)'")]
        public async Task WhenIGetADocumentWithID(string id)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
                string translatedId = this.TranslateId(id);
                Document document = await client.GetAsync(translatedId, translatedId).ConfigureAwait(false);
                this.ScenarioContext.Set(document, ResultKey);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Then("it should match the entity")]
        public void ThenItShouldMatch(Table table)
        {
            SampleEntity expected = table.CreateInstance<SampleEntity>();
            SampleEntity actual = this.ScenarioContext.Get<SampleEntity>(ResultKey);

            IsMatch(expected, actual);
        }

        [Then("it should match the document response")]
        public void WhenItShouldMatchTheDocumentResponse(Table table)
        {
            SampleEntity expected = table.CreateInstance<SampleEntity>();
            DocumentResponse<SampleEntity> actual = this.ScenarioContext.Get<DocumentResponse<SampleEntity>>(ResultKey);

            IsMatch(expected, actual.Document);
        }

        [Then("the document should match the storage result")]
        public void ThenTheDocumentShouldMatchTheStorageResponse()
        {
            ResourceResponse<Document> expectedResponse = this.ScenarioContext.Get<List<ResourceResponse<Document>>>(StorageResultsKey)[0];
            SampleEntity expected = expectedResponse.Resource.As<SampleEntity>();
            Document actualDocument = this.ScenarioContext.Get<Document>(ResultKey);
            SampleEntity actual = actualDocument.As<SampleEntity>();
            Assert.AreEqual(expectedResponse.Resource.ETag, actualDocument.ETag);
            Assert.AreEqual(expectedResponse.Resource.Timestamp, actualDocument.Timestamp);
            IsMatch(expected, actual);
        }

        [Then("the entity instance should match the storage result")]
        public void ThenTheEntityInstanceShouldMatchTheStorageResponse()
        {
            ResourceResponse<Document> expectedResponse = this.ScenarioContext.Get<List<ResourceResponse<Document>>>(StorageResultsKey)[0];
            SampleEntity expected = expectedResponse.Resource.As<SampleEntity>();
            EntityInstance<SampleEntity> actual = this.ScenarioContext.Get<EntityInstance<SampleEntity>>(ResultKey);
            Assert.AreEqual(expectedResponse.Resource.ETag, actual.ETag);
            Assert.AreEqual(expectedResponse.Resource.Timestamp.Ticks, actual.Timestamp.Ticks);
            IsMatch(expected, actual.Entity);
        }

        [When("I get an entity instance with ID '(.*)'")]
        public async Task WhenIGetAnEntityInstanceWithID(string id)
        {
            try
            {
                ICosmosDbSqlClient client = this.FeatureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);

                string translatedId = this.TranslateId(id);
                EntityInstance<SampleEntity> instance = await client.GetEntityInstanceAsync<SampleEntity>(
                    translatedId,
                    translatedId).ConfigureAwait(false);
                this.ScenarioContext.Set(instance, ResultKey);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [When("I query entities with a null specification")]
        public Task WhenIQueryEntitiesWithANullSpecification()
        {
            return this.ExecuteQueryHelper((SqlQuerySpec)null);
        }

        [When("I query entities with a null query string")]
        public Task WhenIQueryEntitiesWithANullQueryString()
        {
            return this.ExecuteQueryHelper((string)null);
        }

        [When("I query entities with the text '(.*)'")]
        public Task WhenIQueryEntitiesWithTheText(string queryText)
        {
            return this.ExecuteQueryHelper(queryText);
        }

        [When("I query entities with the text '(.*)' and supply distinctValueParameters")]
        public Task WhenIQueryEntitiesWithTheTextAndSupplyDistinctValueParameters(string queryText)
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            return this.ExecuteQueryHelper(queryText, entities);
        }

        [When("I query entities with the text '(.*)' and supply a value from a stored entity")]
        public Task WhenIQueryEntitiesWithTheTextAndSupplyAValueFromARandomStoredEntity(string queryText)
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            int value = entities[2].SomeValue;
            this.ScenarioContext.Set(value, QueryValueKey);
            return this.ExecuteQueryHelper(queryText, value);
        }

        [When("I query entities with the text '(.*)' and supply a value that is not stored")]
        public Task WhenIQueryEntitiesWithTheTextAndSupplyAValueThatIsNotStored(string queryText)
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            int value = entities.Max(e => e.SomeValue) + 1;
            this.ScenarioContext.Set(value, QueryValueKey);
            return this.ExecuteQueryHelper(queryText, value);
        }

        [Then("there should be no entities in the response")]
        public void ThenThereShouldBeNoEntitiesInTheResponse()
        {
            Assert.IsEmpty(this.GetAllResults());
        }

        [Then("there should be as many entities in the result set as in the set that was stored")]
        public void ThenThereShouldBeAsManyEntitiesInTheResultSetAsInTheSetThatWasStored()
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            IEnumerable<SampleEntity> result = this.GetAllResults();

            Assert.AreEqual(entities.Count, result.Count());
        }

        [Then("each result should have a matching entity in the set that was stored")]
        public void ThenEachResultShouldHaveAMatchingEntityInTheSetThatWasStored()
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            foreach (SampleEntity result in this.GetAllResults())
            {
                SampleEntity entity = entities.Find(e => e.Id == result.Id);
                Assert.NotNull(entity);
                IsMatch(entity, result);
            }
        }

        [Then("each page of results should have matching entities in the set that was stored")]
        public void ThenEachPageOfResultShouldHaveMatchingEntitiesInTheSetThatWasStored()
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            foreach (FeedResponse<SampleEntity> page in this.ScenarioContext.Get<List<FeedResponse<SampleEntity>>>(ResultKey))
            {
                foreach (SampleEntity result in page)
                {
                    SampleEntity entity = entities.Find(e => e.Id == result.Id);
                    Assert.NotNull(entity);
                    IsMatch(entity, result);
                }
            }
        }

        [When("I query (.*) pages of entities with the text '(.*)' and a page size of (.*)")]
        public async Task WhenIQueryPagesOfEntitiesWithTheTextAndAPageSizeOf(int pageCount, string queryText, int pageSize)
        {
            for (int i = 0; i < pageCount; ++i)
            {
                await this.ExecuteQueryHelper(queryText, pageSize, i).ConfigureAwait(false);
            }
        }

        [When("I query (.*) pages of entities by continuation token with the text '(.*)' and a page size of (.*)")]
        public async Task WhenIQueryPagesOfEntitiesByContinuationTokenWithTheTextAndAPageSizeOf(int pageCount, string queryText, int pageSize)
        {
            FeedResponse<SampleEntity> previousPage = null;

            for (int i = 0; i < pageCount; ++i)
            {
                previousPage = await this.ExecuteQueryHelper(queryText, pageSize, previousPage).ConfigureAwait(false);
            }
        }

        [When("I query (.*) pages of entities with a page size of (.*) with the text '(.*)' and supply distinctValueParameters")]
        public async Task WhenIQueryPagesOfEntitiesWithAPageSizeOfWithTheTextAndSupplyDistinctValueParameters(int pageCount, int pageSize, string queryText)
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);

            for (int i = 0; i < pageCount; ++i)
            {
                await this.ExecuteQueryHelper(queryText, pageSize, i, entities).ConfigureAwait(false);
            }
        }

        [When("I query (.*) pages of entities by continuation token with a page size of (.*) with the text '(.*)' and supply distinctValueParameters")]
        public async Task WhenIQueryPagesOfEntitiesByContinuationTokenWithAPageSizeOfWithTheTextAndSupplyDistinctValueParameters(int pageCount, int pageSize, string queryText)
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);

            FeedResponse<SampleEntity> previousPage = null;

            for (int i = 0; i < pageCount; ++i)
            {
                previousPage = await this.ExecuteQueryHelper(queryText, pageSize, previousPage, entities).ConfigureAwait(false);
            }
        }

        [Then("there should be between (.*) and (.*) entities in each page")]
        public void ThenThereShouldBeBetweenAndEntitiesInEachPage(int lowerInclusiveBound, int upperInclusiveBound)
        {
            foreach (FeedResponse<SampleEntity> page in this.ScenarioContext.Get<List<FeedResponse<SampleEntity>>>(ResultKey))
            {
                Assert.GreaterOrEqual(page.Count, lowerInclusiveBound);
                Assert.LessOrEqual(page.Count, upperInclusiveBound);
            }
        }

        [Then("the total number of entities across the pages should be the same as in the set that was stored")]
        public void ThenTheTotalNumberOfEntitesAcrossThePagesShouldBeTheSameAsInTheSetThatWasStored()
        {
            List<SampleEntity> entities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            var allResults = this.ScenarioContext.Get<List<FeedResponse<SampleEntity>>>(ResultKey)
                .SelectMany(p => p)
                .ToList();
            Assert.AreEqual(entities.Count, allResults.Count);
            Assert.AreEqual(entities.Count, allResults.Distinct().Count());
        }

        [Then("the results should match the entities stored with that value")]
        public void ThenTheResultsShouldMatchTheEntitiesStoredWithThatValue()
        {
            List<SampleEntity> allEntities = this.FeatureContext.Get<List<SampleEntity>>(StoredEntitiesKey);
            int value = this.ScenarioContext.Get<int>(QueryValueKey);

            var entities = allEntities.Where(e => e.SomeValue == value).ToList();

            foreach (SampleEntity result in this.GetAllResults())
            {
                SampleEntity entity = entities.Find(e => e.Id == result.Id);
                Assert.NotNull(entity);
                IsMatch(entity, result);
            }
        }

        /// <inheritdoc/>
        protected override int ParamValueSelector(SampleEntity e) => e.SomeValue;

        private static void IsMatch(SampleEntity expected, SampleEntity actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.SomeValue, actual.SomeValue);
        }

        private static async Task AddRandomEntities(FeatureContext featureContext, int count)
        {
            var entities = new ConcurrentBag<SampleEntity>();
            ICosmosDbSqlClient client = featureContext.Get<ICosmosDbSqlClient>(CosmosDbContextKeys.CosmosDbSqlClient);
            var random = new Random();

            await Enumerable.Range(0, count).ForEachAsync(_ =>
            {
                var entity = new SampleEntity { Id = Guid.NewGuid().ToString(), Name = CryptoString.RandomString(), SomeValue = random.Next() };
                entities.Add(entity);
                return client.UpsertAsync(entity);
            }).ConfigureAwait(false);

            featureContext.Set(entities.ToList(), StoredEntitiesKey);
        }

        private Task StoreAnEntity(SampleEntity item, string etag = null) =>
            this.ExecuteWithClient(client => client.UpsertAsync(item, etag == null ? null : new RequestOptions { AccessCondition = new AccessCondition { Condition = etag, Type = AccessConditionType.IfMatch } }));

        private Task InsertAnEntity(SampleEntity item) =>
            this.ExecuteWithClient(client => client.InsertAsync(item));

        private Task UpdateAnEntity(SampleEntity item, string etag = null) =>
            this.ExecuteWithClient(client => client.UpdateAsync(item, etag));

        private Task ExecuteQueryHelper(SqlQuerySpec querySpec) =>
            this.ExecuteWithClient(client => client.ExecuteQueryAsync<SampleEntity>(querySpec));

        private Task ExecuteQueryHelper(string queryText) => this.ExecuteMultipageQuery((client, feedOptions) =>
            client.ExecuteQueryAsync<SampleEntity>(queryText, feedOptions: feedOptions));

        private string TranslateId(string id)
        {
            switch (id)
            {
                case NullString:
                    return null;
                case NewGuidString:
                    return Guid.NewGuid().ToString();
                default:
                    return id;
            }
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Elements should be documented

