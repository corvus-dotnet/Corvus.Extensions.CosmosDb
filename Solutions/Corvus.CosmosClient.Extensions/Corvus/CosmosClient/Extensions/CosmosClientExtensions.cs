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
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static Task<string?> ForEachAsync<T>(this Container container, string queryText, Action<T> action, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (queryText is null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return ForEachAsync(container, new QueryDefinition(queryText), action, requestOptions, maxBatchCount, continuationToken, cancellationToken);
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
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static async Task<string?> ForEachAsync<T>(this Container container, QueryDefinition queryDefinition, Action<T> action, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (queryDefinition == null)
            {
                throw new ArgumentNullException(nameof(queryDefinition));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            FeedIterator<T> iterator = container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);

            int batchCount = 0;
            string? previousContinuationToken = null;
            string? responseContinuationToken = null;
            try
            {
                while (iterator.HasMoreResults && (!maxBatchCount.HasValue || batchCount < maxBatchCount.Value))
                {
                    batchCount++;

                    FeedResponse<T> response = await Retriable.RetryAsync(
                        () => iterator.ReadNextAsync(cancellationToken),
                        CancellationToken.None,
                        new Backoff(3, TimeSpan.FromSeconds(1)),
                        RetryOnBusyPolicy.Instance,
                        false)
                        .ConfigureAwait(false);

                    previousContinuationToken = responseContinuationToken;
                    responseContinuationToken = response.ContinuationToken;
                    foreach (T item in response)
                    {
                        action(item);
                        cancellationToken.ThrowIfCancellationRequested();
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
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static Task<string?> ForEachAsync<T>(this Container container, string queryText, Func<T, Task> actionAsync, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (queryText is null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            if (actionAsync is null)
            {
                throw new ArgumentNullException(nameof(actionAsync));
            }

            return ForEachAsync(container, new QueryDefinition(queryText), actionAsync, requestOptions, maxBatchCount, continuationToken, cancellationToken);
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
        /// <param name="cancellationToken">A cancellation token to terminate the option early.</param>
        /// <returns>A <see cref="Task"/> which provides a continuation token if it terminates before .</returns>
        public static async Task<string?> ForEachAsync<T>(this Container container, QueryDefinition queryDefinition, Func<T, Task> actionAsync, QueryRequestOptions? requestOptions = null, int? maxBatchCount = null, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (queryDefinition == null)
            {
                throw new ArgumentNullException(nameof(queryDefinition));
            }

            if (actionAsync == null)
            {
                throw new ArgumentNullException(nameof(actionAsync));
            }

            FeedIterator<T> iterator = container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);

            int batchCount = 0;
            string? previousContinuationToken = null;
            string? responseContinuationToken = null;
            try
            {
                while (iterator.HasMoreResults && (!maxBatchCount.HasValue || batchCount < maxBatchCount.Value))
                {
                    batchCount++;

                    FeedResponse<T> response = await Retriable.RetryAsync(
                        () => iterator.ReadNextAsync(cancellationToken),
                        CancellationToken.None,
                        new Backoff(3, TimeSpan.FromSeconds(1)),
                        RetryOnBusyPolicy.Instance,
                        false)
                        .ConfigureAwait(false);

                    previousContinuationToken = responseContinuationToken;
                    responseContinuationToken = response.ContinuationToken;
                    foreach (T item in response)
                    {
                        await actionAsync(item).ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
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