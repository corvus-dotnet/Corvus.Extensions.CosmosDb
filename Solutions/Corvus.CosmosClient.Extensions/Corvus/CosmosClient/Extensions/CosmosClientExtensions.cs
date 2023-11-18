// <copyright file="CosmosClientExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.Retry;
    using Corvus.Retry.Strategies;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Extensions for the cosmos client.
    /// </summary>
    public static class CosmosClientExtensions
    {
        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="container">The Cosmos container against which to execute the query.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="requestOptions">Request options for the query.</param>
        /// <param name="maxBatchCount">The maximum number of batches to process.</param>
        /// <param name="continuationToken">The continuation token from which to resume processing.</param>
        /// <param name="feedRange">The feed range over which to execute the iterator.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static Task<string?> ForEachAsync<T>(this Container container, string queryText, Action<T> action, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, FeedRange? feedRange = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(container);

            ArgumentNullException.ThrowIfNull(queryText);

            ArgumentNullException.ThrowIfNull(action);

            return ForEachAsync(container, new QueryDefinition(queryText), action, requestOptions, maxBatchCount, continuationToken, feedRange, cancellationToken);
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="container">The Cosmos container against which to execute the query.</param>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="requestOptions">Request options for the query.</param>
        /// <param name="maxBatchCount">The maximum number of batches to process.</param>
        /// <param name="continuationToken">The continuation token from which to resume processing.</param>
        /// <param name="feedRange">The feed range over which to execute the iterator.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static async Task<string?> ForEachAsync<T>(this Container container, QueryDefinition queryDefinition, Action<T> action, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, FeedRange? feedRange = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(container);

            ArgumentNullException.ThrowIfNull(queryDefinition);

            ArgumentNullException.ThrowIfNull(action);

            FeedIterator<T> iterator =
                feedRange is FeedRange fr ?
                    container.GetItemQueryIterator<T>(fr, queryDefinition, continuationToken, requestOptions) :
                    container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);

            int batchCount = 0;
            string? previousContinuationToken = null;
            string? responseContinuationToken = null;
            try
            {
                while (iterator.HasMoreResults && (!maxBatchCount.HasValue || batchCount < maxBatchCount.Value))
                {
                    FeedResponse<T> response = await Retriable.RetryAsync(
                        () => iterator.ReadNextAsync(cancellationToken),
                        CancellationToken.None,
                        new Backoff(3, TimeSpan.FromSeconds(1)),
                        RetryOnBusyPolicy.Instance,
                        false)
                        .ConfigureAwait(false);

                    previousContinuationToken = responseContinuationToken;
                    responseContinuationToken = response.ContinuationToken;
                    bool batchHadAtLeastOneItem = false;
                    foreach (T item in response)
                    {
                        batchHadAtLeastOneItem = true;
                        action(item);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (batchHadAtLeastOneItem)
                    {
                        batchCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // We cancelled the operation so we revert to the previous continuation token to allow reprocessing from the previous batch
                responseContinuationToken = previousContinuationToken;
            }

            return responseContinuationToken;
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="container">The Cosmos container against which to execute the query.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="actionAsync">The action to execute.</param>
        /// <param name="requestOptions">Request options for the query.</param>
        /// <param name="maxBatchCount">The maximum number of batches to process.</param>
        /// <param name="continuationToken">The continuation token from which to resume processing.</param>
        /// <param name="feedRange">The feed range over which to execute the iterator.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static Task<string?> ForEachAsync<T>(this Container container, string queryText, Func<T, Task> actionAsync, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, FeedRange? feedRange = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(container);

            ArgumentNullException.ThrowIfNull(queryText);

            ArgumentNullException.ThrowIfNull(actionAsync);

            return ForEachAsync(container, new QueryDefinition(queryText), actionAsync, requestOptions, maxBatchCount, continuationToken, feedRange, cancellationToken);
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="container">The Cosmos container against which to execute the query.</param>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="actionAsync">The action to execute.</param>
        /// <param name="requestOptions">Request options for the query.</param>
        /// <param name="maxBatchCount">The maximum number of batches to process.</param>
        /// <param name="continuationToken">The continuation token from which to resume processing.</param>
        /// <param name="feedRange">The feed range over which to execute the iterator.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static async Task<string?> ForEachAsync<T>(this Container container, QueryDefinition queryDefinition, Func<T, Task> actionAsync, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, FeedRange? feedRange = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(container);

            ArgumentNullException.ThrowIfNull(queryDefinition);

            ArgumentNullException.ThrowIfNull(actionAsync);

            FeedIterator<T> iterator = feedRange is FeedRange fr ?
                container.GetItemQueryIterator<T>(fr, queryDefinition, continuationToken, requestOptions) :
                container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);

            int batchCount = 0;
            string? previousContinuationToken = null;
            string? responseContinuationToken = null;
            try
            {
                while (iterator.HasMoreResults && (!maxBatchCount.HasValue || batchCount < maxBatchCount.Value))
                {
                    FeedResponse<T> response = await Retriable.RetryAsync(
                        () => iterator.ReadNextAsync(cancellationToken),
                        CancellationToken.None,
                        new Backoff(3, TimeSpan.FromSeconds(1)),
                        RetryOnBusyPolicy.Instance,
                        false)
                        .ConfigureAwait(false);

                    previousContinuationToken = responseContinuationToken;
                    responseContinuationToken = response.ContinuationToken;
                    bool batchHadAtLeastOneItem = false;
                    foreach (T item in response)
                    {
                        batchHadAtLeastOneItem = true;
                        await actionAsync(item).ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (batchHadAtLeastOneItem)
                    {
                        batchCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // We cancelled the operation so we revert to the previous continuation token to allow reprocessing from the previous batch
                responseContinuationToken = previousContinuationToken;
            }

            return responseContinuationToken;
        }
    }
}