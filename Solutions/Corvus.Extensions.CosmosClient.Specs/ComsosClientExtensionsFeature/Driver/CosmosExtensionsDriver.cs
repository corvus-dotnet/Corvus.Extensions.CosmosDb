namespace Corvus.Extensions.CosmosClient.Specs.ComsosClientExtensionsFeature.Driver
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.CosmosClient.Specs.Common;
    using Corvus.SpecFlow.Extensions;
    using Microsoft.Azure.Cosmos;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    internal static class CosmosExtensionsDriver
    {
        /// <summary>
        /// Adds the given people to the default container.
        /// </summary>
        /// <param name="containerContext">The feature or scenario context which has the Cosmos container.</param>
        /// <param name="containerKey">The key in the context with which to get the Cosmos Container.</param>
        /// <param name="people">The people to add to the default container.</param>
        /// <returns>A <see cref="Task"/> which completes once the people have been added to the container.</returns>
        internal static Task AddPeopleToContainerAsync(SpecFlowContext containerContext, string containerKey, IList<Person> people)
        {
            return AddPeopleToContainer(GetCosmosContainer(containerContext, containerKey), people);
        }

        /// <summary>
        /// Create a container against a database.
        /// </summary>
        /// <param name="databaseContext">The context from which to get the <see cref="Database"/>.</param>
        /// <param name="containerContext">The context into which to store the <see cref="Container"/>, or null if you do not need to store it.</param>
        /// <param name="containerKey">The key at which to store the <see cref="Container"/>, or null if you do not need to store it.</param>
        /// <returns></returns>
        internal static async Task<Container> CreateContainer(string partitionKeyPath, SpecFlowContext databaseContext, ScenarioContext containerContext = null, string containerKey = null)
        {
            Database database = databaseContext.Get<Database>(CosmosDbContextKeys.CosmosDbDatabase);
            Container container = await database.CreateContainerIfNotExistsAsync("client-" + Guid.NewGuid(), partitionKeyPath);
            if (containerContext != null && containerKey != null)
            {
                containerContext.Set(container, containerKey);
                CosmosDbContextBindings.AddScenarioLevelCosmosDbContainerForCleanup(containerContext, container);
            }
            return container;
        }


        /// <summary>
        /// Adds the given people to the given container.
        /// </summary>
        /// <param name="container">The container to which to add the people.</param>
        /// <param name="people">The list of people to add.</param>
        /// <returns>A <see cref="Task"/> which completes once the people have been added to the container.</returns>
        internal static Task AddPeopleToContainer(Container container, IEnumerable<Person> people)
        {
            return people.ForEachAsync(person => container.CreateItemAsync(person));
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="containerContext">The context from which to get the Cosmos Container.</param>
        /// <param name="containerKey">The key in the context with which to get the Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static Task<IList<T>> IteratePeopleWithSyncMethodAsync<T>(string queryText, SpecFlowContext containerContext, string containerKey, ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            return IteratePeopleWithSyncMethodAsync<T>(queryText, GetCosmosContainer(containerContext, containerKey), scenarioContext, resultsKey);
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="container">The Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static async Task<IList<T>> IteratePeopleWithSyncMethodAsync<T>(string queryText, Container container, ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            var results = new List<T>();
            await container.ForEachAsync<T>(queryText, t => results.Add(t)).ConfigureAwait(false);
            scenarioContext.Set(results, resultsKey);
            return results;
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="containerContext">The context from which to get the Cosmos Container.</param>
        /// <param name="containerKey">The key in the context with which to get the Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static Task<IList<T>> IteratePeopleWithAsyncMethodAsync<T>(string queryText, SpecFlowContext containerContext, string containerKey, ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            return IteratePeopleWithAsyncMethodAsync<T>(queryText, GetCosmosContainer(containerContext, containerKey), scenarioContext, resultsKey);
        }

        /// <summary>
        /// Executes a query and iterates the result with an asynchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="container">The Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static async Task<IList<T>> IteratePeopleWithAsyncMethodAsync<T>(string queryText, Container container, ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            var results = new List<T>();
            await container.ForEachAsync<T>(queryText, t => { results.Add(t); return Task.CompletedTask; }).ConfigureAwait(false);
            scenarioContext.Set(results, resultsKey);
            return results;
        }

        private static Container GetCosmosContainer(SpecFlowContext containerContext, string containerKey = CosmosDbContextKeys.CosmosDbContainer)
        {
            return containerContext.Get<Container>(containerKey);
        }
    }
}
