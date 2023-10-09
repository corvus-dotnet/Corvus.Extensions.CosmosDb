// <copyright file="CosmosExtensionsDriver.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions.Specs.ComsosClientExtensionsFeature.Driver
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.CosmosClient.Extensions;
    using Corvus.CosmosClient.Extensions.Specs.Common;
    using Corvus.Testing.CosmosDb.SpecFlow;

    using Microsoft.Azure.Cosmos;

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
        /// <param name="partitionKeyPath">The partition key path to use when creating the container.</param>
        /// <param name="databaseContext">The context from which to get the <see cref="Database"/>.</param>
        /// <param name="containerContext">The context into which to store the <see cref="Container"/>, or null if you do not need to store it.</param>
        /// <param name="containerKey">The key at which to store the <see cref="Container"/>, or null if you do not need to store it.</param>
        /// <returns>A task that completes when the container has been created.</returns>
        internal static async Task<Container> CreateContainer(string partitionKeyPath, SpecFlowContext databaseContext, ScenarioContext? containerContext = null, string? containerKey = null)
        {
            int? defaultTtl = null;
            if (databaseContext.TryGetValue(CosmosDbContextKeys.ContainerTimeToLive, out string defaultTtlString))
            {
                if (int.TryParse(defaultTtlString, out int parsedDefaultTtl))
                {
                    defaultTtl = parsedDefaultTtl;
                }
            }

            Database database = databaseContext.Get<Database>(CosmosDbContextKeys.CosmosDbDatabase);
            Container container = await database.CreateContainerIfNotExistsAsync(BuildContainerProperties("client-" + Guid.NewGuid(), partitionKeyPath, defaultTtl)).ConfigureAwait(false);

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
        internal static async Task AddPeopleToContainer(Container container, IEnumerable<Person> people)
        {
            foreach (Person person in people)
            {
                await container.CreateItemAsync(person);
            }
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="containerContext">The context from which to get the Cosmos Container.</param>
        /// <param name="containerKey">The key in the context with which to get the Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results.</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="batchSize">The batch size, or null if the default is to be used.</param>
        /// <param name="maxBatchCount">The max batch count, or null if the default is to be used.</param>
        /// <returns>The people found when iterating the query.</returns>
        internal static Task<IList<T>> IteratePeopleWithSyncMethodAsync<T>(string queryText, SpecFlowContext containerContext, string containerKey, ScenarioContext scenarioContext, string? resultsKey = null, int? batchSize = null, int? maxBatchCount = null)
        {
            return IteratePeopleWithSyncMethodAsync<T>(queryText, GetCosmosContainer(containerContext, containerKey), scenarioContext, resultsKey, batchSize, maxBatchCount);
        }

        /// <summary>
        /// Executes a query and iterates the result with a synchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="container">The Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results.</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="batchSize">The batch size, or null if the default is to be used.</param>
        /// <param name="maxBatchCount">The max batch count, or null if the default is to be used.</param>
        /// <returns>The people found when iterating the query.</returns>
        internal static async Task<IList<T>> IteratePeopleWithSyncMethodAsync<T>(string queryText, Container container, ScenarioContext scenarioContext, string? resultsKey = null, int? batchSize = null, int? maxBatchCount = null)
        {
            var results = new List<T>();
            QueryRequestOptions? requestOptions = batchSize.HasValue ? new QueryRequestOptions { MaxItemCount = batchSize } : null;
            await container.ForEachAsync<T>(queryText, t => results.Add(t), requestOptions, maxBatchCount).ConfigureAwait(false);
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
        /// <param name="scenarioContext">The scenario context in which to set the results.</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="batchSize">The batch size, or null if the default is to be used.</param>
        /// <param name="maxBatchCount">The max batch count, or null if the default is to be used.</param>
        /// <returns>The people found when iterating the query.</returns>
        internal static Task<IList<T>> IteratePeopleWithAsyncMethodAsync<T>(string queryText, SpecFlowContext containerContext, string containerKey, ScenarioContext scenarioContext, string? resultsKey = null, int? batchSize = null, int? maxBatchCount = null)
        {
            return IteratePeopleWithAsyncMethodAsync<T>(queryText, GetCosmosContainer(containerContext, containerKey), scenarioContext, resultsKey, batchSize, maxBatchCount);
        }

        /// <summary>
        /// Executes a query and iterates the result with an asynchronous iterator.
        /// </summary>
        /// <typeparam name="T">The type of the entity to iterate.</typeparam>
        /// <param name="queryText">The query text.</param>
        /// <param name="container">The Cosmos Container.</param>
        /// <param name="scenarioContext">The scenario context in which to set the results.</param>
        /// <param name="resultsKey">The key in which to set the results (or null if the results do not need to be set).</param>
        /// <param name="batchSize">The batch size, or null if the default is to be used.</param>
        /// <param name="maxBatchCount">The max batch count, or null if the default is to be used.</param>
        /// <returns>The people found when iterating the query.</returns>
        internal static async Task<IList<T>> IteratePeopleWithAsyncMethodAsync<T>(string queryText, Container container, ScenarioContext scenarioContext, string? resultsKey = null, int? batchSize = null, int? maxBatchCount = null)
        {
            QueryRequestOptions? requestOptions = batchSize.HasValue ? new QueryRequestOptions { MaxItemCount = batchSize } : null;
            var results = new List<T>();
            await container.ForEachAsync<T>(
                queryText,
                t =>
                {
                    results.Add(t);
                    return Task.CompletedTask;
                },
                requestOptions,
                maxBatchCount).ConfigureAwait(false);
            scenarioContext.Set(results, resultsKey);
            return results;
        }

        private static ContainerProperties BuildContainerProperties(string id, string partitionKeyPath, int? ttl)
        {
            // We support hierarchical partition keys if the path contains multiple elements delimited by a semicolon.
            string[] paths = partitionKeyPath.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (paths.Length > 0)
            {
                return new ContainerProperties { Id = id, PartitionKeyPaths = paths, DefaultTimeToLive = ttl };
            }

            return new ContainerProperties { Id = id, PartitionKeyPath = paths[0] };
        }

        private static Container GetCosmosContainer(SpecFlowContext containerContext, string containerKey = CosmosDbContextKeys.CosmosDbContainer)
        {
            return containerContext.Get<Container>(containerKey);
        }
    }
}