namespace Corvus.Extensions.CosmosClient.Specs
{
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.CosmosClient.Specs.Driver;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TechTalk.SpecFlow;

    [Binding]
    public class EntityInstanceFeatureSteps
    {
        public EntityInstanceFeatureSteps(ScenarioContext scenarioContext)
        {
            this.ScenarioContext = scenarioContext;
        }

        public ScenarioContext ScenarioContext { get; }

        [Given(@"I create a Person with Name ""(.*)"" and DateOfBirth ""(.*)"" called ""(.*)""")]
        public void GivenICreateAPersonWithNameAndDateOfBirthCalled(string name, string dateOfBirth, string key)
        {
            EntityInstanceDriver.CreatePerson(name, dateOfBirth, this.ScenarioContext, key);
        }
        
        [Given(@"I serialize the Person ""(.*)"" to a document called ""(.*)"" with ETag ""(.*)""")]
        public void GivenISerializeThePersonToADocumentCalledWithETag(string personKey, string documentKey, string eTag)
        {
            EntityInstanceDriver.SerializePersonToDocument(personKey, eTag, this.ScenarioContext, documentKey);
        }
        
        [When(@"I deserialize the document called ""(.*)"" to an EntityInstance called ""(.*)""")]
        public void WhenIDeserializeTheDocumentCalledToAnEntityInstanceCalled(string documentKey, string entityInstanceKey)
        {
            EntityInstanceDriver.DeserializeEntityInstanceOfPersonFromKey(documentKey, this.ScenarioContext, entityInstanceKey);
        }
        
        [Then(@"the EntityInstance called ""(.*)"" should have an Entity with Name ""(.*)"" and DateOfBirth ""(.*)"" and an ETag ""(.*)""")]
        public void ThenTheEntityInstanceCalledShouldHaveAnEntityWithNameAndDateOfBirthAndAnETag(string entityInstanceKey, string expectedName, string expectedDateOfBirth, string expectedETag)
        {
            EntityInstance<Person> actualEntityInstance = this.ScenarioContext.Get<EntityInstance<Person>>(entityInstanceKey);
            EntityInstanceDriver.MatchEntityInstanceOfPerson(actualEntityInstance, expectedName, expectedDateOfBirth, expectedETag);
        }

        [Given(@"I create an EntityInstance for the Person called ""(.*)"" with ETag ""(.*)"" called ""(.*)""")]
        public void GivenICreateAnEntityInstanceForThePersonCalledWithETagCalled(string personKey, string eTag, string documentKey)
        {
            EntityInstanceDriver.CreateEntityInstance(personKey, eTag, this.ScenarioContext, documentKey);
        }

        [Given(@"I serialize the EntityInstance called ""(.*)"" to a document called ""(.*)""")]
        public void GivenISerializeTheEntityInstanceCalledToADocumentCalled(string entityInstanceKey, string documentKey)
        {
            EntityInstanceDriver.SerializeEntityInstanceToDocument(entityInstanceKey, this.ScenarioContext, documentKey);
        }

    }
}
