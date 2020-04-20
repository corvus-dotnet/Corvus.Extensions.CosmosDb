// <copyright file="EntityInstanceFeatureSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosClient.Specs.EntityInstanceFeature
{
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.CosmosClient.Specs.Common;
    using Corvus.Extensions.CosmosClient.Specs.Common.Driver;
    using Corvus.Extensions.CosmosClient.Specs.EntityInstanceFeature.Driver;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class EntityInstanceFeatureSteps
    {
        public EntityInstanceFeatureSteps(ScenarioContext scenarioContext)
        {
            this.ScenarioContext = scenarioContext;
        }

        public ScenarioContext ScenarioContext { get; }

        [Given(@"I create a Person with Id ""(.*)"", Name ""(.*)"" and DateOfBirth ""(.*)"" called ""(.*)""")]
        public void GivenICreateAPersonWithIdNameAndDateOfBirthCalled(string id, string name, string dateOfBirth, string key)
        {
            PersonDriver.CreatePerson(id, name, dateOfBirth, this.ScenarioContext, key);
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

        [Then(@"the EntityInstance called ""(.*)"" should have an Entity with Id ""(.*)"", Name ""(.*)"" and DateOfBirth ""(.*)"" and an ETag ""(.*)""")]
        public void ThenTheEntityInstanceCalledShouldHaveAnEntityWithNameAndDateOfBirthAndAnETag(string entityInstanceKey, string expectedId, string expectedName, string expectedDateOfBirth, string expectedETag)
        {
            EntityInstance<Person> actualEntityInstance = this.ScenarioContext.Get<EntityInstance<Person>>(entityInstanceKey);
            EntityInstanceDriver.MatchEntityInstanceOfPerson(actualEntityInstance, expectedId, expectedName, expectedDateOfBirth, expectedETag);
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

        [Then(@"the Equals comparison of the EntityInstance called ""(.*)"" with the EntityInstance called ""(.*)"" should be ""(.*)""")]
        public void ThenTheEqualsComparisonOfTheEntityInstanceCalledWithTheEntityInstanceCalledShouldBe(string leftKey, string rightKey, bool comparison)
        {
            Assert.AreEqual(comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey).Equals(this.ScenarioContext.Get<EntityInstance<Person>>(rightKey)));
        }

        [Then(@"the == comparison of the EntityInstance called ""(.*)"" with the EntityInstance called ""(.*)"" should be ""(.*)""")]
        public void ThenTheComparisonOfTheEntityInstanceCalledWithTheEntityInstanceCalledShouldBe(string leftKey, string rightKey, bool comparison)
        {
            Assert.AreEqual(comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey) == this.ScenarioContext.Get<EntityInstance<Person>>(rightKey));
        }

        [Then(@"the != comparison of the EntityInstance called ""(.*)"" with the EntityInstance called ""(.*)"" should be not ""(.*)""")]
        public void ThenTheComparisonOfTheEntityInstanceCalledWithTheEntityInstanceCalledShouldBeNot(string leftKey, string rightKey, bool comparison)
        {
            Assert.AreEqual(!comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey) != this.ScenarioContext.Get<EntityInstance<Person>>(rightKey));
        }

        [Then(@"the Equals comparison of the EntityInstance called ""(.*)"" with the EntityInstance called ""(.*)"" as an object should be ""(.*)""")]
        public void ThenTheEqualsComparisonOfTheEntityInstanceCalledWithTheEntityInstanceCalledAsAnObjectShouldBe(string leftKey, string rightKey, bool comparison)
        {
            Assert.AreEqual(comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey).Equals((object)this.ScenarioContext.Get<EntityInstance<Person>>(rightKey)));
        }

        [Then(@"the Equals comparison of the EntityInstance called ""(.*)"" with a null EntityInstance should be ""(.*)""")]
        public void ThenTheEqualsComparisonOfTheEntityInstanceCalledWithANullEntityInstanceShouldBe(string leftKey, bool comparison)
        {
            Assert.AreEqual(comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey).Equals(null));
        }

        [Then(@"the Equals comparison of the EntityInstance called ""(.*)"" with a null object should be ""(.*)""")]
        public void ThenTheEqualsComparisonOfTheEntityInstanceCalledWithANullObjectShouldBe(string leftKey, bool comparison)
        {
            Assert.AreEqual(comparison, this.ScenarioContext.Get<EntityInstance<Person>>(leftKey).Equals((object?)null));
        }
    }
}
