// <copyright file="EntityInstanceDriver.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions.Specs.EntityInstanceFeature.Driver
{
    using Corvus.CosmosClient.Extensions.Specs.Common;
    using Corvus.CosmosClient.Extensions.Specs.Common.Driver;
    using Corvus.Extensions.Cosmos;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    internal static class EntityInstanceDriver
    {
        /// <summary>
        /// Matches an actual entity instance of a person against.
        /// </summary>
        /// <param name="actualEntityInstance">The actual entity instance to verify.</param>
        /// <param name="expectedId">The expected id of the Person.</param>
        /// <param name="expectedName">The expected name of the Person.</param>
        /// <param name="expectedDateOfBirth">The expected date of birth of the person.</param>
        /// <param name="expectedETag">The expected ETag of the entity instance.</param>
        internal static void MatchEntityInstanceOfPerson(EntityInstance<Person> actualEntityInstance, string expectedId, string expectedName, string expectedDateOfBirth, string expectedETag)
        {
            Assert.AreEqual(expectedId, actualEntityInstance.Entity.Id);
            Assert.AreEqual(ValueUtilities.GetNullableString(expectedName), actualEntityInstance.Entity.Name);
            Assert.AreEqual(ValueUtilities.GetNullableDateTimeOffset(expectedDateOfBirth), actualEntityInstance.Entity.DateOfBirth);
            Assert.AreEqual(ValueUtilities.GetNullableString(expectedETag), actualEntityInstance.ETag);
        }

        /// <summary>
        /// Creates an entity instance from a person and an etag.
        /// </summary>
        /// <param name="personKey">The key in which to find the person.</param>
        /// <param name="eTag">The etag for the instance.</param>
        /// <param name="context">The scenario context.</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The resulting <see cref="EntityInstance{T}"/> for the person.</returns>
        internal static EntityInstance<Person> CreateEntityInstance(string personKey, string eTag, ScenarioContext context, string? keyToSet = null)
        {
            Person person = context.Get<Person>(personKey);
            return CreateEntityInstance(person, eTag, context, keyToSet);
        }

        /// <summary>
        /// Creates an entity instance from a person and an etag.
        /// </summary>
        /// <param name="person">The person to create.</param>
        /// <param name="eTag">The etag for the instance.</param>
        /// <param name="context">The scenario context.</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The resulting <see cref="EntityInstance{T}"/> for the person.</returns>
        internal static EntityInstance<Person> CreateEntityInstance(Person person, string eTag, ScenarioContext? context = null, string? keyToSet = null)
        {
            var entityInstance = new EntityInstance<Person>(person, ValueUtilities.GetNullableString(eTag));

            if (context != null && keyToSet != null)
            {
                context.Set(entityInstance, keyToSet);
            }

            return entityInstance;
        }

        /// <summary>
        /// Serialize a person to a document.
        /// </summary>
        /// <param name="personKey">The key in the context for the person to serialize.</param>
        /// <param name="eTag">The etag for the document.</param>
        /// <param name="context">The scenario context.</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The serialized <see cref="Person"/>.</returns>
        internal static string SerializePersonToDocument(string personKey, string eTag, ScenarioContext context, string? keyToSet = null)
        {
            Person person = context.Get<Person>(personKey);
            return SerializePersonToDocument(person, eTag, context, keyToSet);
        }

        /// <summary>
        /// Serialize a person to a document.
        /// </summary>
        /// <param name="person">The person to serialize.</param>
        /// <param name="eTag">The etag for the document.</param>
        /// <param name="context">The scenario context (or null if you do not wish to set the value into the context).</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The serialized <see cref="Person"/>.</returns>
        internal static string SerializePersonToDocument(Person person, string eTag, ScenarioContext? context = null, string? keyToSet = null)
        {
            string serializedPerson = JsonConvert.SerializeObject(person);
            var jobject = JObject.Parse(serializedPerson);
            jobject["_etag"] = ValueUtilities.GetNullableString(eTag);

            string serializedDocument = jobject.ToString();

            if (context != null && keyToSet != null)
            {
                context.Set(serializedDocument, keyToSet);
            }

            return serializedDocument;
        }

        /// <summary>
        /// Deserialize an entity instance of a person from a document.
        /// </summary>
        /// <param name="documentKey">The key in the scenario context for the document to deserialize.</param>
        /// <param name="context">The scenario context.</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The deserialized <see cref="EntityInstance{T}"/>.</returns>
        internal static EntityInstance<Person> DeserializeEntityInstanceOfPersonFromKey(string documentKey, ScenarioContext context, string? keyToSet = null)
        {
            string document = context.Get<string>(documentKey);
            return DeserializeEntityInstanceOfPerson(document, context, keyToSet);
        }

        /// <summary>
        /// Deserialize an entity instance of a person from a document.
        /// </summary>
        /// <param name="document">The document to deserialize.</param>
        /// <param name="context">The scenario context (or null if you do not wish to set the value into the context).</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The deserialized <see cref="EntityInstance{T}"/>.</returns>
        internal static EntityInstance<Person> DeserializeEntityInstanceOfPerson(string document, ScenarioContext? context = null, string? keyToSet = null)
        {
            EntityInstance<Person> entityInstance = JsonConvert.DeserializeObject<EntityInstance<Person>>(document);

            if (context != null && keyToSet != null)
            {
                context.Set(entityInstance, keyToSet);
            }

            return entityInstance;
        }

        /// <summary>
        /// Serialize an entity instance to a document.
        /// </summary>
        /// <param name="entityInstanceKey">The key in the context for the entityInstance to serialize.</param>
        /// <param name="context">The scenario context.</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The serialized <see cref="EntityInstance{Person}"/>.</returns>
        internal static string SerializeEntityInstanceToDocument(string entityInstanceKey, ScenarioContext context, string? keyToSet = null)
        {
            EntityInstance<Person> entityInstance = context.Get<EntityInstance<Person>>(entityInstanceKey);
            return SerializeEntityInstanceToDocument(entityInstance, context, keyToSet);
        }

        /// <summary>
        /// Serialize an entity instance to a document.
        /// </summary>
        /// <param name="entityInstance">The entityInstance to serialize.</param>
        /// <param name="context">The scenario context (or null if you do not wish to set the value into the context).</param>
        /// <param name="keyToSet">The key to set in the scenario context (or null if you do not wish to set the value into the context).</param>
        /// <returns>The serialized <see cref="EntityInstance{Person}"/>.</returns>
        internal static string SerializeEntityInstanceToDocument(EntityInstance<Person> entityInstance, ScenarioContext? context = null, string? keyToSet = null)
        {
            string serializedDocument = JsonConvert.SerializeObject(entityInstance);

            if (context != null && keyToSet != null)
            {
                context.Set(serializedDocument, keyToSet);
            }

            return serializedDocument;
        }
    }
}