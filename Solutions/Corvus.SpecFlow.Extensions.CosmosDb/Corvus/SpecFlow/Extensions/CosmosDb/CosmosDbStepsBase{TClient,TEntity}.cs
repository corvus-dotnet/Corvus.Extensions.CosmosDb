// <copyright file="CosmosDbStepsBase{TClient,TEntity}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb;
    using Microsoft.Azure.Documents.Client;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Base class for test steps that use Cosmos DB repositories.
    /// </summary>
    /// <typeparam name="TClient">
    /// The client type - either <c>ICosmosDbSqlClient</c> or <c>ICosmsDbGremlinClient</c>.
    /// </typeparam>
    /// <typeparam name="TEntity">The entity type returned by queries.</typeparam>
    public abstract class CosmosDbStepsBase<TClient, TEntity>
    {
        /// <summary>
        /// Key used to store results in the SpecFlow scenario context.
        /// </summary>
        protected const string ResultKey = "Result";

        /// <summary>
        /// Placeholder used in parameterized queries.
        /// </summary>
        protected const string ValueString = "@value";

        private const string BuildDistinctValueParametersString = "{buildDistinctValueParameters}";

        /// <summary>
        /// Create a <see cref="CosmosDbStepsBase{TClient, TEntity}"/>.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test feature context.</param>
        /// <param name="scenarioContext">The SpecFlow test scenario context. </param>
        protected CosmosDbStepsBase(
            FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            this.FeatureContext = featureContext;
            this.ScenarioContext = scenarioContext;
        }

        /// <summary>
        /// Gets the SpecFlow feature context.
        /// </summary>
        protected FeatureContext FeatureContext { get; }

        /// <summary>
        /// Gets the SpecFlow scenario context.
        /// </summary>
        protected ScenarioContext ScenarioContext { get; }

        /// <summary>
        /// Creates query string text with parameters.
        /// </summary>
        /// <param name="queryText">The query.</param>
        /// <param name="entities">The parameter values.</param>
        /// <returns>The string.</returns>
        protected static string CreateParameterizedQueryText(string queryText, List<TEntity> entities)
        {
            return queryText.Replace(BuildDistinctValueParametersString, ParameterHelpers.GetValueList(entities));
        }

        /// <summary>
        /// Used to select parameter values out of entities in some query helpers.
        /// </summary>
        /// <param name="e">The entity.</param>
        /// <returns>The selected value.</returns>
        protected abstract int ParamValueSelector(TEntity e);

        /// <summary>
        /// Execute an operation that requires a client, and store the result in the scenario context
        /// under the <see cref="ResultKey"/> key. If an exception occurs, it will be stored in the scenario
        /// context under the <see cref="CosmosDbContextKeys.ExceptionKey"/> key.
        /// </summary>
        /// <typeparam name="TResult">The type of result that the operation produces.</typeparam>
        /// <param name="action">The operation to invoke.</param>
        /// <returns>A task that completes once the operation has finished.</returns>
        protected async Task ExecuteWithClient<TResult>(Func<TClient, Task<TResult>> action)
        {
            TClient client = this.FeatureContext.Get<TClient>(CosmosDbContextKeys.CosmosDbClient);

            try
            {
                TResult result = await action(client).ConfigureAwait(false);
                this.ScenarioContext.Set(result, ResultKey);
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
            }
        }

        /// <summary>
        /// Execute a query that returns multiple results, re-running it multiple times if it
        /// provides a continuation, and storing the results in the scenario context under the
        /// <see cref="ResultKey"/> key. If an exception occurs, it will be stored in the scenario
        /// context under the <see cref="CosmosDbContextKeys.ExceptionKey"/> key.
        /// </summary>
        /// <param name="executeQuery">Executes the underlying query logic.</param>
        /// <returns>A task that completes when the queries have been completed.</returns>
        protected Task ExecuteMultipageQuery(Func<TClient, FeedOptions, Task<FeedResponse<TEntity>>> executeQuery) => this.ExecuteWithClient(async client =>
        {
            var responses = new List<FeedResponse<TEntity>>();
            for (FeedResponse<TEntity> feedResponse = null; feedResponse == null || feedResponse.ResponseContinuation != null;)
            {
                feedResponse = await executeQuery(client, new FeedOptions { RequestContinuation = feedResponse?.ResponseContinuation, EnableCrossPartitionQuery = true }).ConfigureAwait(false);
                responses.Add(feedResponse);
            }

            return responses;
        });

        /// <summary>
        /// Execute a query that may be executed by the test many times to produce multiple
        /// pages of results.
        /// </summary>
        /// <param name="executeQuery">Executes the underlying query logic.</param>
        /// <returns>A task that completes when the queries have been completed.</returns>
        protected async Task<FeedResponse<TEntity>> ExecuteSingleOfManyPagesQuery(
            Func<TClient, Task<FeedResponse<TEntity>>> executeQuery)
        {
            if (!this.ScenarioContext.TryGetValue(ResultKey, out List<FeedResponse<TEntity>> pages))
            {
                pages = new List<FeedResponse<TEntity>>();
                this.ScenarioContext.Set(pages, ResultKey);
            }

            try
            {
                TClient client = this.FeatureContext.Get<TClient>(CosmosDbContextKeys.CosmosDbClient);
                FeedResponse<TEntity> feedResponse = await executeQuery(client).ConfigureAwait(false);
                pages.Add(feedResponse);
                return feedResponse;
            }
            catch (Exception e)
            {
                this.ScenarioContext.Set(e, CosmosDbContextKeys.ExceptionKey);
                return null;
            }
        }

        /// <summary>
        /// Gets all entities from a multi-page query.
        /// </summary>
        /// <returns>The entities.</returns>
        protected IEnumerable<TEntity> GetAllResults()
        {
            List<FeedResponse<TEntity>> pages = this.ScenarioContext.Get<List<FeedResponse<TEntity>>>(ResultKey);
            return pages.SelectMany(p => p);
        }
    }
}
