// <copyright file="CosmosDbGremlinClient.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb.GremlinQuery;
    using Corvus.Retry;
    using Corvus.Retry.Policies;
    using Gremlin.Net.Driver;
    using Gremlin.Net.Driver.Exceptions;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A client built over CosmosDB Gremlin API.
    /// </summary>
    public class CosmosDbGremlinClient : ICosmosDbGremlinClient
    {
        private readonly Lazy<Task<IGremlinClient>> initializationTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClient"/> class.
        /// </summary>
        /// <param name="sqlClient">An underlying Cosmos DB database and collection client, that the graph will use for storage.</param>
        /// <param name="gremlinHost">The gremlin server host.</param>
        /// <param name="gremlinPort">The gremlin server port.</param>
        /// <param name="useSsl">Indicates whether to use SSL.</param>
        /// <remarks>
        /// The <paramref name="sqlClient"/> must have its storage initialized when it is passed to this method.
        /// </remarks>
        public CosmosDbGremlinClient(ICosmosDbSqlClient sqlClient, string gremlinHost, int gremlinPort, bool useSsl)
        {
            this.SqlClient = sqlClient;
            this.GremlinHost = gremlinHost;
            this.GremlinPort = gremlinPort;
            this.UseSsl = useSsl;
            this.initializationTask = new Lazy<Task<IGremlinClient>>(this.InitializeAsync, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc/>
        public string GremlinHost { get; }

        /// <inheritdoc/>
        public int GremlinPort { get; }

        /// <inheritdoc/>
        public bool UseSsl { get; }

        /// <inheritdoc/>
        public JsonSerializerSettings JsonSerializerSettings => this.SqlClient.JsonSerializerSettings;

        /// <inheritdoc/>
        public ConnectionPolicy DefaultConnectionPolicy => this.SqlClient.DefaultConnectionPolicy;

        /// <inheritdoc/>
        public string Container => this.SqlClient.Container;

        /// <inheritdoc/>
        public string Database => this.SqlClient.Database;

        /// <inheritdoc/>
        public int? DefaultOfferThroughput => this.SqlClient.DefaultOfferThroughput;

        /// <inheritdoc/>
        public bool UseDatabaseThroughput => this.SqlClient.UseDatabaseThroughput;

        /// <inheritdoc/>
        public int? DefaultTimeToLive => this.SqlClient.DefaultTimeToLive;

        /// <inheritdoc/>
        public ConsistencyLevel? DefaultDesiredConsistencyLevel => this.SqlClient.DefaultDesiredConsistencyLevel;

        /// <inheritdoc/>
        public IndexingPolicy DefaultIndexingPolicy => this.SqlClient.DefaultIndexingPolicy;

        /// <inheritdoc/>
        public PartitionKeyDefinition PartitionKeyDefinition => this.SqlClient.PartitionKeyDefinition;

        /// <inheritdoc/>
        public UniqueKeyPolicy DefaultUniqueKeyPolicy => this.SqlClient.DefaultUniqueKeyPolicy;

        private ICosmosDbSqlClient SqlClient { get; }

        /// <inheritdoc/>
        public Task<IReadOnlyCollection<JToken>> ExecuteGremlinQueryAsync(string request, Dictionary<string, object> bindings = null)
        {
            return Retriable.RetryAsync(
                                () => this.ExecuteQueryAsyncWithMeaningfulException(request, bindings),
                                CancellationToken.None,
                                new GremlinClientRetryStrategy(this.DefaultConnectionPolicy.RetryOptions),
                                new AnyException());
        }

        /// <inheritdoc/>
        public Task<JToken> ExecuteGremlinQuerySingleResultAsync(string request, Dictionary<string, object> bindings = null)
        {
            return Retriable.RetryAsync(
                    () => this.ExecuteGremlinQueryWithSingleResultAndMeaningfulException(request, bindings),
                    CancellationToken.None,
                    new GremlinClientRetryStrategy(this.DefaultConnectionPolicy.RetryOptions),
                    new AnyException());
        }

        /// <inheritdoc/>
        public Task<IGremlinClient> GetGremlinClientAsync()
        {
            return this.initializationTask.Value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.initializationTask.IsValueCreated)
            {
                // Dispose the client if it has ever been created
                // There is a chance of an exit deadlock here as we are
                // blocking on the Task. In reality this will only occur if
                // we shut down while we are starting up. And it only occurs when the
                // process is shutting down anyway (we would not normally call Dispose on
                // this singleton until the container is shut down)
                this.initializationTask.Value.Result.Dispose();
            }
        }

        /// <inheritdoc/>
        public Task<DocumentClient> GetDocumentClientAsync()
        {
            return this.SqlClient.GetDocumentClientAsync();
        }

        /// <inheritdoc/>
        public Task<ResourceResponse<Document>> CreateAsync<T>(T item, RequestOptions requestOptions = null)
        {
            return this.SqlClient.CreateAsync(item, requestOptions);
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string id, RequestOptions requestOptions = null)
        {
            return this.SqlClient.DeleteAsync(id, requestOptions);
        }

        /// <inheritdoc/>
        public Task DeleteCollectionAsync()
        {
            return this.SqlClient.DeleteCollectionAsync();
        }

        /// <inheritdoc/>
        public Task<FeedResponse<T>> ExecuteQueryAsync<T>(SqlQuerySpec sqlQuerySpec, int pagesToSkip = 0, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ExecuteQueryAsync<T>(sqlQuerySpec, pagesToSkip, feedOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<FeedResponse<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery, int pagesToSkip = 0, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ExecuteQueryAsync(documentQuery, pagesToSkip, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IOrderedQueryable<T>> CreateDocumentQueryAsync<T>(FeedOptions feedOptions = null)
        {
            return this.SqlClient.CreateDocumentQueryAsync<T>(feedOptions);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, Task<TResult>> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ForEachAsync(sqlQuerySpec, func, feedOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, TResult> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ForEachAsync(sqlQuerySpec, func, feedOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ForEachAsync(sqlQuerySpec, action, feedOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Func<T, Task> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ForEachAsync(sqlQuerySpec, func, feedOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<DocumentResponse<T>> ReadDocumentAsync<T>(string id, RequestOptions requestOptions = null)
        {
            return this.SqlClient.ReadDocumentAsync<T>(id, requestOptions);
        }

        /// <inheritdoc/>
        public Task ReplaceIndexingPolicyAsync(IndexingPolicy newIndexingPolicy = null, Func<long, Task> progressCallback = null, RequestOptions requestOptions = null, int pollIntervalMilliseconds = 1000, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.ReplaceIndexingPolicyAsync(newIndexingPolicy, progressCallback, requestOptions, pollIntervalMilliseconds, cancellationToken);
        }

        /// <inheritdoc/>
        public Task SetThroughputAsync(int rus, bool? enableRUPerMinute = null)
        {
            return this.SqlClient.SetThroughputAsync(rus, enableRUPerMinute);
        }

        /// <inheritdoc/>
        public Task<int> GetThroughputAsync()
        {
            return this.SqlClient.GetThroughputAsync();
        }

        /// <inheritdoc/>
        public Task<ResourceResponse<Document>> UpsertAsync<T>(T item, RequestOptions requestOptions = null)
        {
            return this.SqlClient.UpsertAsync(item, requestOptions);
        }

        /// <inheritdoc/>
        public Task WaitForReindexingAsync(Func<long, Task> progressCallback, int pollIntervalMilliseconds = 1000, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return this.SqlClient.WaitForReindexingAsync(progressCallback, pollIntervalMilliseconds, requestOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public Graph StartTraversal()
        {
            return new Graph(this);
        }

        private async Task<IReadOnlyCollection<JToken>> ExecuteQueryAsyncWithMeaningfulException(string request, Dictionary<string, object> bindings = null)
        {
            IGremlinClient gremlinClient = await this.GetGremlinClientAsync().ConfigureAwait(false);
            try
            {
                return await gremlinClient.SubmitAsync<JToken>(request, bindings).ConfigureAwait(false);
            }
            catch (ResponseException ex)
            {
                throw new GremlinClientException("Unable to execute Gremlin query.", ex);
            }
        }

        private async Task<JToken> ExecuteGremlinQueryWithSingleResultAndMeaningfulException(string request, Dictionary<string, object> bindings = null)
        {
            IGremlinClient gremlinClient = await this.GetGremlinClientAsync().ConfigureAwait(false);
            try
            {
                return await gremlinClient.SubmitWithSingleResultAsync<JToken>(request, bindings).ConfigureAwait(false);
            }
            catch (ResponseException ex)
            {
                throw new GremlinClientException("Unable to execute Gremlin query.", ex);
            }
        }

        private async Task<IGremlinClient> InitializeAsync()
        {
            DocumentClient documentClient = await this.SqlClient.GetDocumentClientAsync().ConfigureAwait(false);
            var gremlinServer = new GremlinServer(this.GremlinHost, this.GremlinPort, this.UseSsl, $"/dbs/{this.SqlClient.Database}/colls/{this.SqlClient.Container}", this.ToPlainText(documentClient.AuthKey));
            return new GremlinClient(gremlinServer, new GraphSONJTokenReader(), mimeType: Gremlin.Net.Driver.GremlinClient.GraphSON2MimeType);
        }

        private string ToPlainText(SecureString source)
        {
            IntPtr returnValue = IntPtr.Zero;
            try
            {
                returnValue = Marshal.SecureStringToGlobalAllocUnicode(source);
                return Marshal.PtrToStringUni(returnValue);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(returnValue);
            }
        }
    }
}
