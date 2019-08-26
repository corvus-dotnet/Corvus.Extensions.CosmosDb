// <copyright file="GraphRepositorySteps.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Elements should be documented
#pragma warning disable IDE0009 // Spurious this or me qualification
#pragma warning disable RCS1192 // Spurious avoid string literals

namespace Endjin.GraphRepository.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.Extensions;
    using Corvus.Extensions.CosmosDb;
    using Corvus.Extensions.CosmosDb.Crypto;
    using Corvus.Extensions.CosmosDb.GremlinQuery;
    using Corvus.SpecFlow.Extensions.CosmosDb;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    using NUnit.Framework;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public class CosmosDbGremlinClientSteps : CosmosDbGremlinClientStepsBase<SampleEntity>
    {
        private const string NullString = "null";
        private const string NewGuidString = "newguid";

        private const string StorageResultsKey = "StorageResult";
        private const string StoredEntitiesKey = "StoredEntities";
        private const string QueryValueKey = "QueryValue";

        public CosmosDbGremlinClientSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
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
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            Assert.IsNotNull(await client.GetDocumentClientAsync().ConfigureAwait(false));
        }

        [Given("I delete an entity with ID '(.*)'")]
        [When("I delete an entity with ID '(.*)'")]
        public async Task WhenIDeleteAnEntityWithID(string id)
        {
            try
            {
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
                string translatedId = this.TranslateId(id);
                await client.DeleteAsync(translatedId, translatedId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given("I add the following edge to the graph")]
        [When("I add the following edge to the graph")]
        public Task WhenIAddTheFollowingEdgeToTheGraph(Table table)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            return client.AddEdgeAsync(table.Rows[0]["Label"], table.Rows[0]["Start Id"], table.Rows[0]["End Id"]);
        }

        [Given("I store (.*) entities with the label '(.*)'")]
        public async Task GivenIStoreEntitiesWithTheLabel(int numberOfEntities, string label)
        {
            var entities = new System.Collections.Concurrent.ConcurrentBag<SampleEntity>();
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            var random = new Random(3);

            await Enumerable.Range(0, numberOfEntities).ForEachAsync(_ =>
            {
                var entity = new SampleEntity { Id = Guid.NewGuid().ToString(), Name = CryptoString.RandomString(), SomeValue = random.Next() };
                entities.Add(entity);
                return client.AddVertexAsync(entity, label);
            }).ConfigureAwait(false);

            this.ScenarioContext.Set(entities.ToList(), StoredEntitiesKey);
        }

        [When("I get all entities with the label '(.*)'")]
        public async Task WhenIGetAllEntitiesWithTheLabelAsync(string label)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            GraphTraversal<dynamic, SampleEntity> traversal = client.StartTraversal().V<SampleEntity>().HasLabel(label);
            IEnumerable<SampleEntity> result = await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
            this.ScenarioContext.Set(result.ToList(), ResultKey);
        }

        [Then("the result should have (.*) entities")]
        public void ThenTheResultShouldHaveEntities(int count)
        {
            IList<SampleEntity> result = this.ScenarioContext.Get<IList<SampleEntity>>(ResultKey);
            Assert.AreEqual(count, result.Count);
        }

        [Then("the following out traversals should exist")]
        public async Task ThenTheFollowingOutTraversalsShouldExist(Table table)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            foreach (TableRow row in table.Rows)
            {
                string startId = row["Start Id"];
                string endId = row["End Id"];
                string label = row["Label"];

                GraphTraversal<dynamic, Edge> traversal = client.StartTraversal().V(startId).OutE(label);
                IEnumerable<Edge> edges = await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
                var peopleList = edges.ToList();
                Assert.AreEqual(1, peopleList.Count);
                Edge actual = peopleList[0];
                Assert.AreEqual(label, actual.Label);
            }
        }

        [Then("the traversal called '(.*)' should be empty")]
        public async Task ThenTheTraversalCalledShouldBeEmpty(string traversalName)
        {
            GraphTraversal<object, SamplePerson> traversal = this.FeatureContext.Get<GraphTraversal<object, SamplePerson>>(traversalName);
            IEnumerable<SamplePerson> actualList = await traversal.Client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
            Assert.IsFalse(actualList.Any());
        }

        [When("I get the nodes with property '(.*)' as a traversal called '(.*)'")]
        public void WhenIGetTheNodesWithPropertyAsATraversalCalled(string property, string traversalName)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

            GraphTraversal<dynamic, SamplePerson> traversal = client.StartTraversal().V<SamplePerson>().Has(property);

            this.FeatureContext.Set(traversal, traversalName);
        }

        [When("I get the nodes with property '(.*)' as a traversal called '(.*)' with the predicate (.*)")]
        public void WhenIGetTheNodesWithPropertyAsATraversalCalledWithThePredicateBetween(string property, string traversalName, string predicate)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

            GraphTraversal<dynamic, SamplePerson> traversal = client.StartTraversal().V<SamplePerson>().Has(property, predicate);

            this.FeatureContext.Set(traversal, traversalName);
        }

        [Then("the following in traversals should exist")]
        public async Task ThenTheFollowingInTraversalsShouldExist(Table table)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            foreach (TableRow row in table.Rows)
            {
                string startId = row["Start Id"];
                string endId = row["End Id"];
                string label = row["Label"];

                GraphTraversal<dynamic, Edge> traversal = client.StartTraversal().V(startId).InE(label);
                IEnumerable<Edge> edges = await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
                var peopleList = edges.ToList();
                Assert.AreEqual(1, peopleList.Count);
                Edge actual = peopleList[0];
                Assert.AreEqual(label, actual.Label);
            }
        }

        [Given("I set the offer throughput to (.*) ru/s")]
        [When("I set the offer throughput to (.*) ru/s")]
        public async Task WhenISetTheOfferThroughputToRuS(int rus)
        {
            try
            {
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
                await client.SetThroughputAsync(rus).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given("I add the following vertices to the graph with label '(.*)'")]
        [When("I add the following vertices to the graph with label '(.*)'")]
        public async Task WhenIAddTheFollowingVerticesToTheGraphWithLabel(string label, Table table)
        {
            try
            {
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

                foreach (SamplePerson person in table.CreateSet<SamplePerson>())
                {
                    await client.AddVertexAsync(person, label).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given("I update the following vertices in the graph")]
        [When("I update the following vertices in the graph")]
        public async Task WhenIUpdateTheFollowingVerticesInTheGraph(Table table)
        {
            try
            {
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
                foreach (SamplePerson person in table.CreateSet<SamplePerson>())
                {
                    await client.UpdateVertexAsync(person).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        [Given("I get the nodes with label '(.*)' as a traversal called '(.*)'")]
        [When("I get the nodes with label '(.*)' as a traversal called '(.*)'")]
        public void WhenIGetTheNodesWithLabelAsATraversalCalled(string label, string traversalName)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

            GraphTraversal<dynamic, SamplePerson> traversal = client.StartTraversal().V<SamplePerson>().HasLabel(label);

            this.FeatureContext.Set(traversal, traversalName);
        }

        [Given("I fold the traversal called '(.*)' to a list traversal called '(.*)'")]
        [When("I fold the traversal called '(.*)' to a list traversal called '(.*)'")]
        public void WhenIFoldTheTraversalToAListToATraversalCalledCalled(string inputTraversalName, string outputTraversalName)
        {
            GraphTraversal<dynamic, SamplePerson> inputTraversal = this.FeatureContext.Get<GraphTraversal<dynamic, SamplePerson>>(inputTraversalName);
            GraphTraversal<dynamic, SamplePerson> outputTraversal = inputTraversal.Fold();
            this.FeatureContext.Set(outputTraversal, outputTraversalName);
        }

        [When("I execute the list traversal called '(.*)' and store the result in a list called '(.*)'")]
        public async Task WhenIExecuteTheListTraversalCalledAndStoreTheResultInAListCalled(string traversalName, string listName)
        {
            GraphTraversal<object, SamplePerson> traversal = this.FeatureContext.Get<GraphTraversal<object, SamplePerson>>(traversalName);
            IEnumerable<SamplePerson> list = await traversal.Client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
            this.FeatureContext.Set(list.ToList(), listName);
        }

        [Then("I should be able to get the following vertices from the traversal called '(.*)'")]
        public async Task ThenIShouldBeAbleToGetTheFollowingVerticesFromTheTraversalCalledAsync(string traversalName, Table table)
        {
            GraphTraversal<object, SamplePerson> traversal = this.FeatureContext.Get<GraphTraversal<object, SamplePerson>>(traversalName);
            IList<SamplePerson> actualList = (await traversal.Client.ExecuteTraversalAsync(traversal).ConfigureAwait(false)).ToList();
            var expectedList = table.CreateSet<SamplePerson>().ToList();
            CollectionAssert.AreEquivalent(expectedList, actualList);
            //Assert.AreEqual(expectedList.Count, actualList.Count);
            //expectedList.ForEach(expected =>
            //{
            //    Assert.IsTrue(actualList.Any(actual => expected.Equals(actual)));
            //});
            //actualList.ForEach(actual=>
            //{
            //    Assert.IsTrue(expectedList.Any(expected => actual.Equals(expected)));
            //});
        }

        [Then("the list called '(.*)' should be empty")]
        public void ThenTheListCalledShouldBeEmpty(string listName)
        {
            IList<SamplePerson> actualList = this.FeatureContext.Get<IList<SamplePerson>>(listName);
            Assert.AreEqual(0, actualList.Count);
        }

        [Then("I should be able to get the following vertices from the list called '(.*)'")]
        public void ThenIShouldBeAbleToGetTheFollowingVerticesFromTheListCalled(string listName, Table table)
        {
            var expectedList = table.CreateSet<SamplePerson>().ToList();
            IList<SamplePerson> actualList = this.FeatureContext.Get<IList<SamplePerson>>(listName);

            VerifyLists(expectedList, actualList);
        }

        [Then("I should be able to get the following vertices")]
        public async Task ThenIShouldBeAbleToGetTheFollowingNodes(Table table)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

            foreach (SamplePerson expected in table.CreateSet<SamplePerson>())
            {
                SamplePerson actual = await client.GetVertexAsync<SamplePerson>(expected.Id).ConfigureAwait(false);
                VertifyPerson(expected, actual);
            }
        }

        [Then("the result should be (.*) ru/s")]
        public async Task ThenTheResultShouldBeRuS(int rus)
        {
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            int throughput = await client.GetThroughputAsync().ConfigureAwait(false);
            Assert.AreEqual(rus, throughput);
        }

        [Then("it should throw an '(.*)'")]
        [Then("it should throw a '(.*)'")]
        public void ThenItShouldThrowAn(string exceptionTypeName)
        {
            var exceptionType = Type.GetType(exceptionTypeName);
            if (this.ScenarioContext.TryGetValue<Exception>(CosmosDbContextKeys.ExceptionKey, out Exception exception))
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
            if (this.ScenarioContext.TryGetValue<Exception>(CosmosDbContextKeys.ExceptionKey, out Exception exception))
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

        [Then("the GremlinClientException should have an HTTP status code of '(.*)'")]
        public void ThenTheGremlinClientExceptionShouldHaveAnHTTPStatusCodeOf(string statusCode)
        {
            if (this.ScenarioContext.TryGetValue<Exception>(CosmosDbContextKeys.ExceptionKey, out Exception exception))
            {
                if (exception is GremlinClientException graphRepositoryException)
                {
                    var httpStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCode, true);
                    Assert.AreEqual(httpStatusCode, graphRepositoryException.StatusCode);
                }
                else
                {
                    Assert.True(false, "The exception was not a GremlinClientException. (Have you verified with \"It should throw a 'Corvus.Extensions.CosmosDb.GraphRespositoryException'\"?)");
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
            if (this.ScenarioContext.TryGetValue<Exception>(CosmosDbContextKeys.ExceptionKey, out Exception exception))
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
            ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

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
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
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
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
                string translatedId = this.TranslateId(id);
                DocumentResponse<SampleEntity> document = await client.ReadDocumentAsync<SampleEntity>(translatedId, translatedId).ConfigureAwait(false);
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
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
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
                ICosmosDbGremlinClient client = this.FeatureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);

                string translatedId = this.TranslateId(id);
                EntityInstance<SampleEntity> instance = await client.GetEntityInstanceAsync<SampleEntity>(translatedId, translatedId).ConfigureAwait(false);
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
            return this.ExecuteQueryHelper(null);
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
            IEnumerable<SampleEntity> entities = this.GetAllResults();
            Assert.IsEmpty(entities);
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
            List<FeedResponse<SampleEntity>> pages = this.ScenarioContext.Get<List<FeedResponse<SampleEntity>>>(ResultKey);

            foreach (FeedResponse<SampleEntity> page in pages)
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
                await this.ExecuteQueryPageHelper(queryText, pageSize, i).ConfigureAwait(false);
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

        private static void VertifyPerson(SamplePerson expected, SamplePerson actual)
        {
            Assert.IsNotNull(actual, $"Unable to get the vertex with ID '{expected.Id}'");
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
            Assert.AreEqual(expected.DateOfBirth, actual.DateOfBirth);
            Assert.AreEqual(expected.Rating, actual.Rating);
        }

        private static void IsMatch(SampleEntity expected, SampleEntity actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.SomeValue, actual.SomeValue);
        }

        private static void VerifyLists(List<SamplePerson> expectedList, IList<SamplePerson> actualList)
        {
            Assert.AreEqual(expectedList.Count, actualList.Count);
            foreach (SamplePerson expected in expectedList)
            {
                SamplePerson actual = actualList.SingleOrDefault(p => p.Id == expected.Id);
                Assert.IsNotNull(actual);
                VertifyPerson(expected, actual);
            }
        }

        private static async Task AddRandomEntities(FeatureContext featureContext, int count)
        {
            var entities = new System.Collections.Concurrent.ConcurrentBag<SampleEntity>();
            ICosmosDbGremlinClient client = featureContext.Get<ICosmosDbGremlinClient>(CosmosDbContextKeys.CosmosDbClient);
            var random = new Random(3);

            await Enumerable.Range(0, count).ForEachAsync(_ =>
            {
                var entity = new SampleEntity { Id = Guid.NewGuid().ToString(), Name = CryptoString.RandomString(), SomeValue = random.Next() };
                entities.Add(entity);
                return client.UpsertAsync(entity);
            }).ConfigureAwait(false);

            featureContext.Set(entities.ToList(), StoredEntitiesKey);
        }

        private Task StoreAnEntity(SampleEntity item, string etag = null) => this.ExecuteWithClient(client =>
            client.UpsertAsync(item, etag == null ? null : new RequestOptions { AccessCondition = new AccessCondition { Condition = etag, Type = AccessConditionType.IfMatch } }));

        private Task InsertAnEntity(SampleEntity item) => this.ExecuteWithClient(client => client.InsertAsync(item));

        private Task UpdateAnEntity(SampleEntity item, string etag = null) => this.ExecuteWithClient(client => client.UpdateAsync(item, etag));

        private Task ExecuteQueryHelper(SqlQuerySpec querySpec) => this.ExecuteWithClient(async client =>
        {
            var responses = new List<FeedResponse<SampleEntity>>();
            FeedResponse<SampleEntity> feedResponse = null;
            for (int page = 0; feedResponse == null || feedResponse.ResponseContinuation != null; ++page)
            {
                feedResponse = await client.ExecuteQueryAsync<SampleEntity>(querySpec).ConfigureAwait(false);
            }

            return feedResponse;
        });

        private Task ExecuteQueryPageHelper(string queryText, int pageSize, int pageIndex)
            => this.ExecuteSingleOfManyPagesQuery((client) =>
                client.ExecuteQueryAsync<SampleEntity>(
                    queryText,
                    pagesToSkip: pageIndex,
                    feedOptions: new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = true }));

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
#pragma warning restore IDE0009 // Spurious this or me qualification
#pragma warning restore RCS1192 // Spurious avoid string literals
