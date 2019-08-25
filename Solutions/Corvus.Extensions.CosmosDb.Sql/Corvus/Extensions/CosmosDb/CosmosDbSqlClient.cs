// <copyright file="CosmosDbSqlClient.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.Retry;
    using Corvus.Retry.Policies;
    using Corvus.Retry.Strategies;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation of <see cref="ICosmosDbSqlClient"/>.
    /// </summary>
    public class CosmosDbSqlClient : ICosmosDbSqlClient
    {
        private readonly Uri accountUri;
        private readonly string accountKey;
        private readonly Lazy<Task<DocumentClient>> initializationTask;
        private string containerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClient"/> class.
        /// </summary>
        /// <param name="clientDefinition">The details of the client to construct.</param>
        /// <param name="accountUri">The URI of the Cosmos DB.</param>
        /// <param name="accountKey">The Key for the Cosmos DB account.</param>
        /// <param name="jsonSerializerSettings">Cosmos DB JSON serializer settings.</param>
        /// <param name="partitionKeyDefinition">The Cosmos DB partition key definition.</param>
        /// <param name="indexingPolicy">Cosmos DB custom indexing policy.</param>
        /// <param name="uniqueKeyPolicy">Cosmos DB custom unique key policy.</param>
        /// <param name="connectionPolicy">Cosmos DB connection policy.</param>
        /// <param name="desiredConsistencyLevel">Cosmos DB desired default consistency level.</param>
        /// <param name="defaultTimeToLive">The Cosmos DB default time to live for documents in the container.</param>
        /// <param name="defaultOfferThroughput">Set default throughput RUs.</param>
        /// <param name="useDatabaseThroughput">Use database-level throughput rather than container-level throughput.</param>
        public CosmosDbSqlClient(CosmosDbSqlClientDefinition clientDefinition, Uri accountUri, string accountKey, JsonSerializerSettings jsonSerializerSettings = null, PartitionKeyDefinition partitionKeyDefinition = null, IndexingPolicy indexingPolicy = null, UniqueKeyPolicy uniqueKeyPolicy = null, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null, int? defaultTimeToLive = null, int defaultOfferThroughput = 400, bool useDatabaseThroughput = false)
            : this(
                clientDefinition.Database,
                clientDefinition.Container,
                accountUri,
                accountKey,
                jsonSerializerSettings,
                partitionKeyDefinition,
                indexingPolicy,
                uniqueKeyPolicy,
                connectionPolicy,
                desiredConsistencyLevel,
                defaultTimeToLive,
                defaultOfferThroughput,
                useDatabaseThroughput)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClient"/> class.
        /// </summary>
        /// <param name="clientDefinition">The details of the client to construct.</param>
        /// <param name="clientConfiguration">The Cosmos DB client configuration.</param>
        /// <param name="accountKey">The Key for the Cosmos DB account.</param>
        /// <param name="jsonSerializerSettings">Cosmos DB JSON serializer settings.</param>
        public CosmosDbSqlClient(CosmosDbSqlClientDefinition clientDefinition, ICosmosDbSqlClientConfiguration clientConfiguration, string accountKey, JsonSerializerSettings jsonSerializerSettings = null)
            : this(
                clientDefinition.Database,
                clientDefinition.Container,
                clientConfiguration.AccountUri,
                accountKey,
                jsonSerializerSettings,
                clientConfiguration.PartitionKeyDefinition,
                clientConfiguration.IndexingPolicy,
                clientConfiguration.UniqueKeyPolicy,
                clientConfiguration.ConnectionPolicy,
                clientConfiguration.DesiredConsistencyLevel,
                clientConfiguration.DefaultTimeToLive,
                clientConfiguration.DefaultOfferThroughput,
                clientConfiguration.UseDatabaseThroughput)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClient"/> class.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="container">The container name.</param>
        /// <param name="clientConfiguration">The Cosmos DB client configuration.</param>
        /// <param name="accountKey">The Key for the Cosmos DB account.</param>
        /// <param name="jsonSerializerSettings">Cosmos DB JSON serializer settings.</param>
        public CosmosDbSqlClient(string database, string container, ICosmosDbSqlClientConfiguration clientConfiguration, string accountKey, JsonSerializerSettings jsonSerializerSettings = null)
            : this(
                database,
                container,
                clientConfiguration.AccountUri,
                accountKey,
                jsonSerializerSettings,
                clientConfiguration.PartitionKeyDefinition,
                clientConfiguration.IndexingPolicy,
                clientConfiguration.UniqueKeyPolicy,
                clientConfiguration.ConnectionPolicy,
                clientConfiguration.DesiredConsistencyLevel,
                clientConfiguration.DefaultTimeToLive,
                clientConfiguration.DefaultOfferThroughput,
                clientConfiguration.UseDatabaseThroughput)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClient"/> class.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="container">The container name.</param>
        /// <param name="accountUri">The URI of the Cosmos DB.</param>
        /// <param name="accountKey">The Key for the Cosmos DB account.</param>
        /// <param name="jsonSerializerSettings">Cosmos DB JSON serializer settings.</param>
        /// <param name="partitionKeyDefinition">The Cosmos DB partition key definition.</param>
        /// <param name="indexingPolicy">The Cosmos DB indexing policy.</param>
        /// <param name="uniqueKeyPolicy">The Cosmos DB unique key policy.</param>
        /// <param name="connectionPolicy">Cosmos DB connection policy.</param>
        /// <param name="desiredConsistencyLevel">Cosmos DB desired default consistency level.</param>
        /// <param name="defaultTimeToLive">The Cosmos DB default time to live for documents in the container.</param>
        /// <param name="defaultOfferThroughput">Set default throughput RUs.</param>
        /// <param name="useDatabaseThroughput">Use database-level throughput rather than container-level throughput.</param>
        public CosmosDbSqlClient(string database, string container, Uri accountUri, string accountKey, JsonSerializerSettings jsonSerializerSettings = null, PartitionKeyDefinition partitionKeyDefinition = null, IndexingPolicy indexingPolicy = null, UniqueKeyPolicy uniqueKeyPolicy = null, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null, int? defaultTimeToLive = null, int defaultOfferThroughput = 400, bool useDatabaseThroughput = false)
        {
            this.Database = database;
            this.Container = container;
            this.accountUri = accountUri;
            this.accountKey = accountKey;
            this.JsonSerializerSettings = jsonSerializerSettings;
            this.PartitionKeyDefinition = partitionKeyDefinition;
            this.DefaultIndexingPolicy = indexingPolicy;
            this.DefaultUniqueKeyPolicy = uniqueKeyPolicy;
            this.DefaultConnectionPolicy = connectionPolicy ?? ConnectionPolicy.Default;
            this.DefaultDesiredConsistencyLevel = desiredConsistencyLevel;
            this.DefaultTimeToLive = defaultTimeToLive;
            this.DefaultOfferThroughput = defaultOfferThroughput;
            this.UseDatabaseThroughput = useDatabaseThroughput;
            this.initializationTask = new Lazy<Task<DocumentClient>>(() => this.InitializeAsync(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc/>
        public string Database { get; }

        /// <inheritdoc/>
        public string Container { get; }

        /// <inheritdoc/>
        public JsonSerializerSettings JsonSerializerSettings { get; }

        /// <inheritdoc/>
        public PartitionKeyDefinition PartitionKeyDefinition { get; }

        /// <inheritdoc/>
        public IndexingPolicy DefaultIndexingPolicy { get; }

        /// <inheritdoc/>
        public UniqueKeyPolicy DefaultUniqueKeyPolicy { get; }

        /// <inheritdoc/>
        public ConnectionPolicy DefaultConnectionPolicy { get; }

        /// <inheritdoc/>
        public ConsistencyLevel? DefaultDesiredConsistencyLevel { get; }

        /// <inheritdoc/>
        public int? DefaultTimeToLive { get; }

        /// <inheritdoc/>
        public int? DefaultOfferThroughput { get; }

        /// <inheritdoc/>
        public bool UseDatabaseThroughput { get; }

        /// <inheritdoc/>
        public Task<DocumentClient> GetDocumentClientAsync()
        {
            return this.initializationTask.Value;
        }

        /// <inheritdoc/>
        public async Task<ResourceResponse<Document>> CreateAsync<T>(T item, RequestOptions requestOptions = null)
        {
#pragma warning disable RCS1165 // Unconstrained type parameter checked for null.
            if (item == null)
#pragma warning restore RCS1165 // Unconstrained type parameter checked for null.
            {
                throw new ArgumentNullException(nameof(item));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            return await documentClient.CreateDocumentAsync(this.CreateDocumentCollectionUri(), item, requestOptions).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ResourceResponse<Document>> UpsertAsync<T>(T item, RequestOptions requestOptions = null)
        {
#pragma warning disable RCS1165 // Unconstrained type parameter checked for null.
            if (item == null)
#pragma warning restore RCS1165 // Unconstrained type parameter checked for null.
            {
                throw new ArgumentNullException(nameof(item));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            return await documentClient.UpsertDocumentAsync(this.CreateDocumentCollectionUri(), item, requestOptions).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<DocumentResponse<T>> ReadDocumentAsync<T>(string id, RequestOptions requestOptions = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            return await documentClient.ReadDocumentAsync<T>(this.CreateDocumentUri(id), requestOptions).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id, RequestOptions requestOptions = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            await documentClient.DeleteDocumentAsync(this.CreateDocumentUri(id), requestOptions).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (sqlQuerySpec == null)
            {
                throw new ArgumentNullException(nameof(sqlQuerySpec));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            IDocumentQuery<T> queryable = documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                sqlQuerySpec,
                feedOptions).AsDocumentQuery();

            while (queryable.HasMoreResults)
            {
                foreach (T item in await queryable.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false))
                {
                    action(item);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, TResult> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (sqlQuerySpec == null)
            {
                throw new ArgumentNullException(nameof(sqlQuerySpec));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            IDocumentQuery<T> queryable = documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                sqlQuerySpec,
                feedOptions).AsDocumentQuery();

            var results = new List<TResult>();

            while (queryable.HasMoreResults)
            {
                foreach (T item in await queryable.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false))
                {
                    results.Add(func(item));
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Func<T, Task> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (sqlQuerySpec == null)
            {
                throw new ArgumentNullException(nameof(sqlQuerySpec));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            IDocumentQuery<T> queryable = documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                sqlQuerySpec,
                feedOptions).AsDocumentQuery();

            while (queryable.HasMoreResults)
            {
                foreach (T item in await queryable.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false))
                {
                    await func(item).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, Task<TResult>> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (sqlQuerySpec == null)
            {
                throw new ArgumentNullException(nameof(sqlQuerySpec));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            IDocumentQuery<T> queryable = documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                sqlQuerySpec,
                feedOptions).AsDocumentQuery();

            var results = new List<TResult>();
            while (queryable.HasMoreResults)
            {
                foreach (T item in await queryable.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false))
                {
                    results.Add(await func(item).ConfigureAwait(false));
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<FeedResponse<T>> ExecuteQueryAsync<T>(SqlQuerySpec sqlQuerySpec, int pagesToSkip = 0, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (sqlQuerySpec == null)
            {
                throw new ArgumentNullException(nameof(sqlQuerySpec));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            IDocumentQuery<T> queryable = documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                sqlQuerySpec,
                feedOptions)
            .AsDocumentQuery();

            while (queryable.HasMoreResults)
            {
                FeedResponse<T> response = await queryable.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (pagesToSkip > 0)
                {
                    pagesToSkip--;
                    continue;
                }

                return response;
            }

            return new FeedResponse<T>(Enumerable.Empty<T>());
        }

        /// <inheritdoc/>
        public async Task<IOrderedQueryable<T>> CreateDocumentQueryAsync<T>(FeedOptions feedOptions = null)
        {
            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            return documentClient.CreateDocumentQuery<T>(
                this.CreateDocumentCollectionUri(),
                feedOptions);
        }

        /// <inheritdoc/>
        public async Task<FeedResponse<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery, int pagesToSkip = 0, CancellationToken cancellationToken = default)
        {
            if (documentQuery == null)
            {
                throw new ArgumentNullException(nameof(documentQuery));
            }

            await this.EnsureInitializedAsync().ConfigureAwait(false);

            while (documentQuery.HasMoreResults)
            {
                FeedResponse<T> response = await documentQuery.ExecuteNextAsync<T>(cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (pagesToSkip > 0)
                {
                    pagesToSkip--;
                    continue;
                }

                return response;
            }

            return new FeedResponse<T>(Enumerable.Empty<T>());
        }

        /// <inheritdoc/>
        public async Task ReplaceIndexingPolicyAsync(IndexingPolicy newIndexingPolicy = null, Func<long, Task> progressCallback = null, RequestOptions requestOptions = null, int pollIntervalMilliseconds = 1000, CancellationToken cancellationToken = default)
        {
            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            ResourceResponse<DocumentCollection> collectionResponse = await documentClient.ReadDocumentCollectionAsync(this.CreateDocumentCollectionUri(), requestOptions).ConfigureAwait(false);

            if (collectionResponse.IndexTransformationProgress != -1)
            {
                // An index transformation operation is already underway so this is forbidden.
                throw new InvalidOperationException();
            }

            DocumentCollection container = collectionResponse.Resource;
            container.IndexingPolicy = newIndexingPolicy ?? new IndexingPolicy();

            await documentClient.ReplaceDocumentCollectionAsync(collectionResponse.Resource).ConfigureAwait(false);

            await this.WaitForReindexingAsync(progressCallback, pollIntervalMilliseconds, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task WaitForReindexingAsync(Func<long, Task> progressCallback, int pollIntervalMilliseconds = 1000, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            if (progressCallback == null)
            {
                throw new NullReferenceException(nameof(progressCallback));
            }

            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            ResourceResponse<DocumentCollection> containerResponse = await documentClient.ReadDocumentCollectionAsync(this.CreateDocumentCollectionUri(), requestOptions).ConfigureAwait(false);

            if (containerResponse.IndexTransformationProgress == -1)
            {
                // We are immediately complete
                return;
            }

            long complete = 0L;
            while (complete < 100 && complete != -1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ResourceResponse<DocumentCollection> readCollectionResponse = await documentClient.ReadDocumentCollectionAsync(this.CreateDocumentCollectionUri(), requestOptions).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                complete = readCollectionResponse.IndexTransformationProgress;
                if (progressCallback != null)
                {
                    try
                    {
                        await progressCallback(complete).ConfigureAwait(false);
                    }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                    catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                    {
                        // Guaranteed to be safe; swallow any exceptions
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromMilliseconds(pollIntervalMilliseconds)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteCollectionAsync()
        {
            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            await documentClient.DeleteDocumentCollectionAsync(this.CreateDocumentCollectionUri()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetThroughputAsync(int rus, bool? enableRUPerMinute = null)
        {
            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            FeedResponse<Offer> result = this.UseDatabaseThroughput ? await this.GetDatabaseOfferResultAsync(documentClient).ConfigureAwait(false) : await this.GetCollectionOfferResultAsync(documentClient).ConfigureAwait(false);

            await SetThroughputCoreAsync(rus, enableRUPerMinute, documentClient, result).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<int> GetThroughputAsync()
        {
            DocumentClient documentClient = await this.GetDocumentClientAsync().ConfigureAwait(false);

            FeedResponse<Offer> responseFeed = this.UseDatabaseThroughput ? await this.GetDatabaseOfferResultAsync(documentClient).ConfigureAwait(false) : await this.GetCollectionOfferResultAsync(documentClient).ConfigureAwait(false);

            dynamic result = responseFeed.FirstOrDefault();
            return result.Content.OfferThroughput;
        }

        private static async Task SetThroughputCoreAsync(int rus, bool? enableRUPerMinute, DocumentClient documentClient, FeedResponse<Offer> result)
        {
            // Set the throughput
            var offer = new OfferV2(result.FirstOrDefault(), rus, enableRUPerMinute);

            // Now persist these changes to the database by replacing the original resource
            await documentClient.ReplaceOfferAsync(offer).ConfigureAwait(false);
        }

        private async Task<FeedResponse<Offer>> GetCollectionOfferResultAsync(DocumentClient documentClient)
        {
            ResourceResponse<DocumentCollection> collection = await documentClient.ReadDocumentCollectionAsync(this.CreateDocumentCollectionUri()).ConfigureAwait(false);
            IDocumentQuery<Offer> queryable = documentClient.CreateOfferQuery()
                .Where(r => r.ResourceLink == collection.Resource.SelfLink)
                .AsDocumentQuery();

            return await queryable.ExecuteNextAsync<Offer>().ConfigureAwait(false);
        }

        private async Task<FeedResponse<Offer>> GetDatabaseOfferResultAsync(DocumentClient documentClient)
        {
            ResourceResponse<Database> collection = await documentClient.ReadDatabaseAsync(this.CreateDatabaseUri()).ConfigureAwait(false);
            IDocumentQuery<Offer> queryable = documentClient.CreateOfferQuery()
                .Where(r => r.ResourceLink == collection.Resource.SelfLink)
                .AsDocumentQuery();

            return await queryable.ExecuteNextAsync<Offer>().ConfigureAwait(false);
        }

        private Uri CreateDocumentCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(this.Database, this.BuildCollectionName());
        }

        private Uri CreateDatabaseUri()
        {
            return UriFactory.CreateDatabaseUri(this.Database);
        }

        private Uri CreateDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(this.Database, this.BuildCollectionName(), id);
        }

        private string BuildCollectionName()
        {
            return this.containerName ?? (this.containerName = this.Container);
        }

        private async Task<DocumentClient> InitializeAsync()
        {
            DocumentClient documentClient = this.JsonSerializerSettings == null
               ? new DocumentClient(this.accountUri, this.accountKey, this.DefaultConnectionPolicy, this.DefaultDesiredConsistencyLevel)
               : new DocumentClient(this.accountUri, this.accountKey, this.JsonSerializerSettings, this.DefaultConnectionPolicy, this.DefaultDesiredConsistencyLevel);

            RequestOptions databaseRequestOptions = this.UseDatabaseThroughput ? new RequestOptions { OfferThroughput = this.DefaultOfferThroughput } : null;

            await Retriable.RetryAsync(
                () => documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = this.Database }, databaseRequestOptions),
                CancellationToken.None,
                new Backoff(3, TimeSpan.FromSeconds(1)),
                RetryOnBusyPolicy.Instance,
                false)
                .ConfigureAwait(false);

            await this.ValidateDatabaseThroughputAsync(documentClient).ConfigureAwait(false);

            var documentCollection = new DocumentCollection { Id = this.BuildCollectionName(), DefaultTimeToLive = this.DefaultTimeToLive };

            if (this.DefaultIndexingPolicy != null)
            {
                documentCollection.IndexingPolicy = this.DefaultIndexingPolicy;
            }

            if (this.PartitionKeyDefinition != null)
            {
                documentCollection.PartitionKey = this.PartitionKeyDefinition;
            }

            if (this.DefaultUniqueKeyPolicy != null)
            {
                documentCollection.UniqueKeyPolicy = this.DefaultUniqueKeyPolicy;
            }

            var collectionRequestOptions = new RequestOptions { ConsistencyLevel = this.DefaultDesiredConsistencyLevel };

            if (!this.UseDatabaseThroughput)
            {
                collectionRequestOptions.OfferThroughput = this.DefaultOfferThroughput;
            }

            await Retriable.RetryAsync(
                () => documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(this.Database),
                    documentCollection,
                    collectionRequestOptions),
                CancellationToken.None,
                new Backoff(3, TimeSpan.FromSeconds(1)),
                RetryOnBusyPolicy.Instance,
                false)
                .ConfigureAwait(false);

            return documentClient;
        }

        private async Task ValidateDatabaseThroughputAsync(DocumentClient documentClient)
        {
            if (this.UseDatabaseThroughput)
            {
                FeedResponse<Offer> responseFeed = await this.GetDatabaseOfferResultAsync(documentClient).ConfigureAwait(false);
                dynamic result = responseFeed.FirstOrDefault();
                int? offerThroughput = result?.Content.OfferThroughput;
                if (!offerThroughput.HasValue)
                {
                    throw new InvalidOperationException("Unable to use database throughput in a database that has already been created.");
                }

                // Increase the offer throughput if necessary.
                if (offerThroughput.Value < this.DefaultOfferThroughput.Value)
                {
                    await SetThroughputCoreAsync(this.DefaultOfferThroughput.Value, null, documentClient, responseFeed).ConfigureAwait(false);
                }
            }
        }

        private Task EnsureInitializedAsync()
        {
            return this.initializationTask.Value;
        }

        private class RetryOnBusyPolicy : IRetryPolicy
        {
            public static RetryOnBusyPolicy Instance { get; } = new RetryOnBusyPolicy();

            public bool CanRetry(Exception exception) => exception is DocumentClientException dcx && dcx.StatusCode == HttpStatusCode.ServiceUnavailable;
        }
    }
}
