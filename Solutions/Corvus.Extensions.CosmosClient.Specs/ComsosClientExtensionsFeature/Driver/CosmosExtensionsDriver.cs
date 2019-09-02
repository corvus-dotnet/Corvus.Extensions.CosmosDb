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
        /// <param name="people">The people to add to the default container.</param>
        /// <returns>A <see cref="Task"/> which completes once the people have been added to the container.</returns>
        internal static Task AddPeopleToContainerAsync(SpecFlowContext containerContext, IList<Person> people)
        {
            return AddPeopleToContainer(GetCosmosContainer(containerContext), people);
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
        /// <param name="queryText">The query text.</param>
        /// <param name="containerContext">The context from which to get the Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static Task<IList<Person>> IteratePeopleWithSyncMethodAsync(string queryText, SpecFlowContext containerContext,  ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            return IteratePeopleWithSyncMethodAsync(queryText, GetCosmosContainer(containerContext), scenarioContext, resultsKey); 
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="container">The Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set)</param>
        /// <returns></returns>
        internal static async Task<IList<Person>> IteratePeopleWithSyncMethodAsync(string queryText, Container container, ScenarioContext scenarioContext = null, string resultsKey = null)
        {
            var results = new List<Person>();
            await container.ForEachAsync<Person>(queryText, t => results.Add(t)).ConfigureAwait(false);
            scenarioContext.Set(results, resultsKey);
            return results;
        }
        /// <summary>
        /// Executes a query against a container.
        /// </summary>
        /// <param name="queryText">The query to execute.</param>
        /// <param name="containerContext">The feature or scenario context which has the Comsos container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results (or null if you do not need to set the results).</param>
        /// <param name="resultKey">The key against which to set the results (or null if you do not need to set the results).</param>
        /// <returns>A <see cref="Task"/> which, when complete, provides the <see cref="FeedIterator{T}"/> for the query.</returns>
        internal static FeedIterator<Person> ExecutePersonQuery(string queryText, SpecFlowContext containerContext, ScenarioContext scenarioContext = null, string resultKey = null)
        {
            FeedIterator<Person> results = GetCosmosContainer(containerContext).GetItemQueryIterator<Person>(queryText);

            if (scenarioContext != null && resultKey != null)
            {
                scenarioContext.Set(results, resultKey);
            }

            return results;
        }

        private static Container GetCosmosContainer(SpecFlowContext containerContext)
        {
            return containerContext.Get<Container>(CosmosDbContextKeys.CosmosDbContainer);
        }
    }
}
