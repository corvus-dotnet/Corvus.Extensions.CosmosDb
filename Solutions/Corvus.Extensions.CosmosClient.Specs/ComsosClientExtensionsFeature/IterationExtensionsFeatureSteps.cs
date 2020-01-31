namespace Corvus.Extensions.CosmosClient.Specs.ComsosClientExtensionsFeature
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.CosmosClient.Specs.Common;
    using Corvus.Extensions.CosmosClient.Specs.Common.Driver;
    using Corvus.Extensions.CosmosClient.Specs.ComsosClientExtensionsFeature.Driver;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class IterationExtensionsFeatureSteps
    {
        public IterationExtensionsFeatureSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.FeatureContext = featureContext;
            this.ScenarioContext = scenarioContext;
        }

        public FeatureContext FeatureContext { get; }

        public ScenarioContext ScenarioContext { get; }

        [Given(@"that I create a Cosmos Container called ""(.*)""")]
        public Task GivenThatICreateACosmosContainerCalled(string containerKey)
        {
            return CosmosExtensionsDriver.CreateContainer("/id", this.FeatureContext, this.ScenarioContext, containerKey);
        }

        [Given(@"I add a collection of Person objects called ""(.*)"" to the Cosmos Container called ""(.*)""")]
        public Task GivenIAddACollectionOfPersonObjectsCalledToTheCosmosContainer(string peopleKey, string containerKey, Table table)
        {
            IList<Person> people = PersonDriver.CreatePeople(table, this.ScenarioContext, peopleKey);
            return CosmosExtensionsDriver.AddPeopleToContainerAsync(this.ScenarioContext, containerKey, people);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a synchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithASynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithSyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a batch size of ""(.*)"" and a synchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithASynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, int batchSize, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithSyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey, batchSize);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a batch size of ""(.*)"", a max batch count of ""(.*)"" and a synchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithASynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, int batchSize, int maxBatchCount, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithSyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey, batchSize, maxBatchCount);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with an asynchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithAnAsynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithAsyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a batch size of ""(.*)"" and an asynchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithAnAsynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, int batchSize, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithAsyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey, batchSize);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a batch size of ""(.*)"", a max batch count of ""(.*)"" and an asynchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithAnAsynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string containerKey, int batchSize, int maxBatchCount, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithAsyncMethodAsync<Person>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey, batchSize, maxBatchCount);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with a synchronous action and store the Entity Instance of Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithASynchronousActionAndStoreTheEntityInstanceOfPersonObjectsSeenIn(string queryText, string containerKey, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithSyncMethodAsync<EntityInstance<Person>>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey);
        }

        [When(@"I iterate the query ""(.*)"" against the container called ""(.*)"" with an asynchronous action and store the Entity Instance of Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithAnAsynchronousActionAndStoreTheEntityInstanceOfPersonObjectsSeenIn(string queryText, string containerKey, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithAsyncMethodAsync<EntityInstance<Person>>(queryText, this.ScenarioContext, containerKey, this.ScenarioContext, resultsKey);
        }

        [Then(@"the Person collection ""(.*)"" should contain the following items from the Person collection ""(.*)""")]
        public void ThenThePersonCollectionShouldContainTheFollowingItemsFromThePersonCollection(string actualKey, string sourceKey, Table indices)
        {
            IList<Person> actualList = this.ScenarioContext.Get<IList<Person>>(actualKey);
            IList<Person> sourceList = this.ScenarioContext.Get<IList<Person>>(sourceKey);
            IList<Person> expectedList = PersonDriver.GetPeopleFromIndices(sourceList, indices);

            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [Then(@"the Entity Instance of Person collection ""(.*)"" should contain the following items from the Person collection ""(.*)""")]
        public void ThenTheEntityInstanceOfPersonCollectionShouldContainTheFollowingItemsFromThePersonCollection(string actualKey, string sourceKey, Table indices)
        {
            IList<EntityInstance<Person>> entityInstanceList = this.ScenarioContext.Get<IList<EntityInstance<Person>>>(actualKey);
            IList<Person> sourceList = this.ScenarioContext.Get<IList<Person>>(sourceKey);
            IList<Person> expectedList = PersonDriver.GetPeopleFromIndices(sourceList, indices);

            CollectionAssert.AreEqual(expectedList, entityInstanceList.Select(e => e.Entity).ToList());
        }
    }
}