// <copyright file="CosmosDbGremlinClientStepsBase{TEntity}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

////namespace Corvus.SpecFlow.Extensions.CosmosDb
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Threading.Tasks;
////    using Corvus.Repository;
////    using Microsoft.Azure.Documents;
////    using Microsoft.Azure.Documents.Client;
////    using TechTalk.SpecFlow;

////    /// <summary>
////    /// Base class for SpecFlow steps that use a graph-based Cosmos DB store.
////    /// </summary>
////    /// <typeparam name="TEntity">The entity type returned by queries.</typeparam>
////    public abstract class CosmosDbGremlinClientStepsBase<TEntity> : CosmosDbStepsBase<IGraphRepository, TEntity>
////    {
////        /// <summary>
////        /// Create a <see cref="DocumentRepositoryStepsBase{TEntity}"/>.
////        /// </summary>
////        /// <param name="featureContext">The SpecFlow test feature context.</param>
////        /// <param name="scenarioContext">The SpecFlow test scenario context. </param>
////        protected GraphRepositoryStepsBase(
////            FeatureContext featureContext,
////            ScenarioContext scenarioContext)
////            : base(featureContext, scenarioContext)
////        {
////        }

////        /// <summary>
////        /// Execute a string-based query.
////        /// </summary>
////        /// <param name="queryText">The query.</param>
////        /// <returns>Completes when the query has been executed.</returns>
////        protected Task ExecuteQueryHelper(string queryText) => this.ExecuteMultipageQuery((client, feedOptions) =>
////            client.ExecuteQueryAsync<TEntity>(queryText, feedOptions: feedOptions));

////        /// <summary>
////        /// Executes a query for a particular page.
////        /// </summary>
////        /// <param name="queryText">The query to execute.</param>
////        /// <param name="pageSize">The maximum results per page.</param>
////        /// <param name="pageIndex">The page at which to start.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected Task ExecuteQueryHelper(string queryText, int pageSize, int pageIndex)
////            => this.ExecuteSingleOfManyPagesQuery((client) =>
////            client.ExecuteQueryAsync<TEntity>(
////                queryText,
////                pagesToSkip: pageIndex,
////                feedOptions: new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = true }));

////        /// <summary>
////        /// Execute a page-based query.
////        /// </summary>
////        /// <param name="queryText">The query to execute.</param>
////        /// <param name="pageSize">The maximum results per page.</param>
////        /// <param name="previousPage">The response from the preceding page query.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected Task<FeedResponse<TEntity>> ExecuteQueryHelper(string queryText, int pageSize, FeedResponse<TEntity> previousPage)
////            => this.ExecuteSingleOfManyPagesQuery(client =>
////                client.ExecuteQueryAsync<TEntity>(
////                    queryText,
////                    feedOptions: new FeedOptions
////                    {
////                        RequestContinuation = previousPage?.ResponseContinuation,
////                        MaxItemCount = pageSize,
////                        EnableCrossPartitionQuery = true,
////                    }));

////        /// <summary>
////        /// Execute a page-based query.
////        /// </summary>
////        /// <param name="queryText">The query to execute.</param>
////        /// <param name="pageSize">The maximum results per page.</param>
////        /// <param name="previousPage">The response from the preceding page query.</param>
////        /// <param name="entities">Entity values for the query.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected async Task<FeedResponse<TEntity>> ExecuteQueryHelper(
////            string queryText,
////            int pageSize,
////            FeedResponse<TEntity> previousPage,
////            List<TEntity> entities)
////        {
////            var querySpec = new SqlQuerySpec(CreateParameterizedQueryText(queryText, entities), ParameterHelpers.GetParameterCollection(entities.Select(this.ParamValueSelector)));
////            return await this.ExecuteSingleOfManyPagesQuery(client =>
////                client.ExecuteQueryAsync<TEntity>(
////                    querySpec,
////                    feedOptions: new FeedOptions
////                    {
////                        RequestContinuation = previousPage?.ResponseContinuation,
////                        MaxItemCount = pageSize,
////                        EnableCrossPartitionQuery = true,
////                    }))
////                .ConfigureAwait(false);
////        }

////        /// <summary>
////        /// Execute a parameterized query.
////        /// </summary>
////        /// <param name="queryText">The query.</param>
////        /// <param name="value">The parameter value.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected Task ExecuteQueryHelper(string queryText, int value)
////        {
////            if (queryText.IndexOf(ValueString) < 0)
////            {
////                throw new ArgumentException("Query should contain " + ValueString, nameof(queryText));
////            }

////            return this.ExecuteMultipageQuery((client, feedOptions) =>
////                client.ExecuteQueryAsync<TEntity>(
////                    queryText,
////                    new SqlParameterCollection { new SqlParameter(ValueString, value) },
////                    feedOptions: feedOptions));
////        }

////        /// <summary>
////        /// Executes a query with entities.
////        /// </summary>
////        /// <param name="queryText">Query text.</param>
////        /// <param name="entities">Entity values.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected Task ExecuteQueryHelper(string queryText, List<TEntity> entities) => this.ExecuteMultipageQuery((client, feedOptions) =>
////        {
////            var querySpec = new SqlQuerySpec(CreateParameterizedQueryText(queryText, entities), ParameterHelpers.GetParameterCollection(entities.Select(this.ParamValueSelector)));

////            return client.ExecuteQueryAsync<TEntity>(querySpec, feedOptions);
////        });

////        /// <summary>
////        /// Execute a page-based query.
////        /// </summary>
////        /// <param name="queryText">The query to execute.</param>
////        /// <param name="pageSize">The maximum results per page.</param>
////        /// <param name="pageIndex">The page offset.</param>
////        /// <param name="entities">Entity values for the query.</param>
////        /// <returns>A task that completes when the query has run.</returns>
////        protected async Task ExecuteQueryHelper(
////            string queryText,
////            int pageSize,
////            int pageIndex,
////            List<TEntity> entities)
////        {
////            var querySpec = new SqlQuerySpec(CreateParameterizedQueryText(queryText, entities), ParameterHelpers.GetParameterCollection(entities.Select(this.ParamValueSelector)));
////            await this.ExecuteSingleOfManyPagesQuery(client =>
////                client.ExecuteQueryAsync<TEntity>(
////                    querySpec,
////                    pagesToSkip: pageIndex,
////                    feedOptions: new FeedOptions { MaxItemCount = pageSize, EnableCrossPartitionQuery = true }))
////                .ConfigureAwait(false);
////        }
////    }
////}
