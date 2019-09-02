namespace Corvus.Extensions.CosmosClient.Specs.ComsosClientExtensionsFeature
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
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

        [Given(@"I add a collection of Person objects called ""(.*)"" to the Cosmos Container")]
        public Task GivenIAddACollectionOfPersonObjectsCalledToTheCosmosContainer(string peopleKey, Table table)
        {
            IList<Person> people = PersonDriver.CreatePeople(table, this.ScenarioContext, peopleKey);
            return CosmosExtensionsDriver.AddPeopleToContainerAsync(this.FeatureContext, people);
        }

        [When(@"I iterate the query ""(.*)"" with a synchronous action and store the Person objects seen in ""(.*)""")]
        public Task WhenIIterateTheQueryWithASynchronousActionAndStoreThePersonObjectsSeenIn(string queryText, string resultsKey)
        {
            return CosmosExtensionsDriver.IteratePeopleWithSyncMethodAsync(queryText, this.FeatureContext, this.ScenarioContext, resultsKey);
        }

        [Then(@"the Person collection ""(.*)"" should contain the following items from the Person collection ""(.*)""")]
        public void ThenThePersonCollectionShouldContainTheFollowingItemsFromThePersonCollection(string actualKey, string sourceKey, Table indices)
        {
            IList<Person> actualList = this.ScenarioContext.Get<IList<Person>>(actualKey);
            IList<Person> sourceList = this.ScenarioContext.Get<IList<Person>>(sourceKey);
            IList<Person> expectedList = PersonDriver.GetPeopleFromIndices(sourceList, indices);

            CollectionAssert.AreEqual(expectedList, actualList);
        }

    }
}
