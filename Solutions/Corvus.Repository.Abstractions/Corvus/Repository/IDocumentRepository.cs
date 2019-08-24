// <copyright file="IDocumentRepository.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// A repository built over CosmosDB.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides a utility wrapper over the Cosmos DB <see cref="DocumentClient"/> offering
    /// a simple repository implementation.
    /// </para>
    /// <para>
    /// Typically you will just new up an instance and pass it the relevant configuration. This should be a singleton
    /// and reused for all access for this particular repository configuration in a particular host instance.
    /// </para>
    /// <para>
    /// Note that this is intrinsically a single tenant. Multi-tenancy support is layered
    /// on top using the <c>Endjin.Repository.Configuration</c> and <c>Endjin.Repository.Tenancy</c> libraries.
    /// </para>
    /// <para>
    /// The <see cref="IDocumentRepository"/> itself offers a very simple set of APIs for <see cref="UpsertAsync{T}(T, RequestOptions)"/>,
    /// <see cref="DeleteAsync(string, RequestOptions)"/>, <see cref="ExecuteQueryAsync{T}(SqlQuerySpec, int, FeedOptions, CancellationToken)"/>,
    /// <see cref="ReadDocumentAsync{T}(string, RequestOptions)"/> (which gets a document by ID).
    /// </para>
    /// <para>
    /// You will observe that these return the low-level <see cref="ResourceResponse{TResource}"/>, <see cref="FeedResponse{T}"/>, or <see cref="DocumentResponse{TDocument}"/> types.
    /// </para>
    /// <para>
    /// To make them easier to work with, we provide a number of extension methods in <see cref="RepositoryExtensions"/> which can convert from a <see cref="Document"/> to a particular concrete type,
    /// or to our <see cref="EntityInstance{T}"/> type which decorates a concrete type with the ETag and timestamp of the entity. In production code, you would expect to use the <see cref="EntityInstance{T}"/>
    /// overloads most commonly, to support your consistency model.
    /// </para>
    /// <para>
    /// You will also notice that the query APIs offer both paging and continuation token-based APIs. We strongly recommend using continuation tokens for performance reasons. Note that continuation tokens
    /// can be cached (and even serialized!) by the client, and passed back to the framework at an arbitrary future time. This allows you to use them for forward/back style paging in a highly optimized fashion.
    /// We also provide continuation-token style APIs that allow you to skip a number of pages to allow you start the query at an arbitrary number of pages into the result set (or at a page for which you have not
    /// yet cached the continuation token).
    /// </para>
    /// <para>
    /// We also have an optimized special case for iterating a result set with a customized <see cref="ForEachAsync{T}(SqlQuerySpec, Action{T}, FeedOptions, CancellationToken)"/>
    /// method which executes an action over the whole result set, in batches. The action itself may be synchronous or asynchronous, and may produce a result or not.
    /// </para>
    /// <para>
    /// It supports serialization of the complete configuration of a document client
    /// (via the <see cref="RepositoryDefinition"/> and <see cref="RepositoryConfiguration"/> types).
    /// You can opt in to this by using the <see cref="RepositoryServiceCollectionExtensions.AddRepositoryJsonConverters"/> extension
    /// to configure the custom <see cref="JsonConverter"/>.
    /// </para>
    /// <para>
    /// You can store multiple different document types in the same repository - the results are only coerced to a particular dotnet type
    /// when you ask for it. You can also configure your queries to return partial result sets (or use Document DB stored procedures) and map them
    /// to entirely different types from those you added. (See the example below.)
    /// </para>
    /// <para>
    /// However, you should consider whether you want to do this, or whether you would prefer to use Cosmos DB's database-level throughput.
    /// </para>
    /// <para>
    /// When creating the <see cref="IDocumentRepository"/> you provide an additional parameter to specify that you would like to create the database with DB level throughput.
    /// </para>
    /// <para>
    /// If you do so, then supplied default offer throughput is applied to the DB instead of your collection.
    /// </para>
    /// <para>
    /// If the database was already created, <em>without</em> db-level throughput, then an <see cref="InvalidOperationException"/> is thrown.
    /// </para>
    /// <para>
    /// If you create a collection into a DB with db-level throughput, with explicit collection level throughput, then it will get its own allocated throughput within that DB.
    /// </para>
    /// <para>
    /// If you create a collection with db-level throughput in an existing DB with a lower level of throughput, it will <em>increase</em> the throughput. If it is lower, it will leave it alone.
    /// </para>
    /// <para>
    /// Note that you must create repositories with a partition key if they are in a DB-level collection (even when the size/throughput is small) - you will get an explanatory <see cref="DocumentClientException"/> if you don't.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// First, construct an instance of the repository. In this example, we are configuring 1000RU/s, and a direct connection. We are also using endjin's custom serialization support for content type resolution. If the database and collection do not exist,
    /// then they will be created for you. The Cosmos DB instance itself must already exist.
    /// </para>
    /// <para>
    /// <code>
    ///  var repository = new DocumentRepository("ADD_YOUR_DATABASE_NAME_HERE", "ADD_YOUR_CONTAINER_NAME_HERE", new Uri("ADD_YOUR_COSMOS_DB_INSTANCE_URI_HERE"), "ADD_YOUR_CONNECTION_STRING_HERE", SerializerSettings.CreateSerializationSettings(), connectionPolicy: new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct }, defaultOfferThroughput: 10000);
    /// </code>
    /// </para>
    /// <para>
    /// Then we have a couple of different POCOs we want to store in the repository. Note that they are not based on the same type.
    /// We are, however, providing a "discriminator" property that can distinguish between the two types. In our example, we are just
    /// using the simple name of the type; in production code you would typically use the ContentType pattern from Corvus.ContentHandling.
    /// </para>
    /// <para>
    /// <code>
    /// public class FirstClass
    /// {
    ///     public string Id { get; set; } = Guid.NewGuid().ToString();
    ///     public string SomeValue { get; set; }
    ///     public string Discriminator =&gt; return nameof(FirstClass);
    /// }
    ///
    /// public class SecondClass
    /// {
    ///     public string Id { get; set; } = Guid.NewGuid().ToString();
    ///     public int SomeValue { get; set; }
    ///     public string Discriminator =&gt; return nameof(SecondClass);
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Now, if we want to add a couple of instances of that type to the repository we simply call <see cref="UpsertAsync{T}(T, RequestOptions)"/>.
    /// </para>
    /// <para>
    /// <code>
    /// await repository.UpsertAsync(new FirstClass { Id = "1", SomeValue = "hello" });
    /// await repository.UpsertAsync(new FirstClass { Id = "2", SomeValue = "dolly" });
    /// await repository.UpsertAsync(new SecondClass { Id = "3", SomeValue = 1 });
    /// await repository.UpsertAsync(new SecondClass { Id = "4", SomeValue = 2 });
    /// </code>
    /// </para>
    /// <para>
    /// We can now get the items back by ID, strongly typed to our result.
    /// </para>
    /// <para>
    /// <code>
    /// var firstClass = await repository.ReadDocumentAsync&lt;FirstClass&gt;("1");
    /// var secondClass = await repository.ReadDocumentAsync&lt;SecondClass&gt;("2");
    /// </code>
    /// </para>
    /// <para>
    /// Or, we could query all the <c>FirstClass</c> results using the discriminator.
    /// </para>
    /// <para>
    /// <code>
    /// var querySpec = new SqlQuerySpec($"SELECT * FROM entities e WHERE e.discriminator = '{nameof(FirstClass)}'");
    /// var feedResponse = await repository.ExecuteQueryAsync&lt;FirstClass&gt;(querySpec);
    /// </code>
    /// </para>
    /// <para>
    /// We can also force an insert rather than an upsert with <see cref="RepositoryExtensions.InsertAsync{T}(IDocumentRepository, T, RequestOptions)"/>.
    /// </para>
    /// <para>
    /// <code>
    /// await repository.InsertAsync(new FirstClass { Id = "5", SomeValue = "cheerio" });
    /// </code>
    /// </para>
    /// <para>
    /// Similarly, you use Update semantics by getting your object with an etag - as an <see cref="EntityInstance{T}"/>.
    /// </para>
    /// <para>
    /// <code>
    /// var entityInstance = await repository.GetEntityInstanceAsync("4");
    /// entityInstance.Entity.SomeValue = "A very formal greeting.";
    /// // This succeeds as the etags match (assuming no-one else has updated it behind our backs)
    /// await repository.UpdateAsync(entityInstance);
    /// // This fails as the etags no longer match (we didn't stash the result)
    /// entityInstance.Entity.SomeValue = "A less formal 'yo'.";
    /// await repository.UpdateAsync(entityInstance);
    /// </code>
    /// </para>
    /// <para>
    /// If you want, you can manage that etag by hand.
    /// </para>
    /// <para>
    /// <code>
    /// var entityInstance = await repository.GetEntityInstanceAsync("4");
    /// entityInstance.Entity.SomeValue = "A very formal greeting.";
    /// var originalETag = entityInstance.ETag;
    /// // This succeeds as the etags match (assuming no-one else has updated it behind our backs)
    /// await repository.UpdateAsync(entityInstance.Entity, originalETag);
    /// // This fails as the etags no longer match (we didn't stash the result)
    /// entityInstance.Entity.SomeValue = "A less formal 'yo'.";
    /// await repository.UpdateAsync(entityInstance.Entity, originalETag);
    /// </code>
    /// </para>
    /// </example>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Gets the connection policy for the repository.
        /// </summary>
        ConnectionPolicy DefaultConnectionPolicy { get; }

        /// <summary>
        /// Gets the container name.
        /// </summary>
        string Container { get; }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Gets the default throughput RUs for the repository.
        /// </summary>
        int? DefaultOfferThroughput { get; }

        /// <summary>
        /// Gets a value indicating whether the repository is using database-level throughput.
        /// </summary>
        bool UseDatabaseThroughput { get; }

        /// <summary>
        /// Gets the Cosmos DB document default time to live.
        /// </summary>
        int? DefaultTimeToLive { get; }

        /// <summary>
        /// Gets the desired consistency level for the repository.
        /// </summary>
        ConsistencyLevel? DefaultDesiredConsistencyLevel { get; }

        /// <summary>
        /// Gets the indexing policy for the repository.
        /// </summary>
        IndexingPolicy DefaultIndexingPolicy { get; }

        /// <summary>
        /// Gets the JsonSerializerSettings for the repository.
        /// </summary>
        JsonSerializerSettings JsonSerializerSettings { get; }

        /// <summary>
        /// Gets the partition key path.
        /// </summary>
        /// <remarks>
        /// This the Cosmos DB partition for data partitioning.
        /// </remarks>
        PartitionKeyDefinition PartitionKeyDefinition { get; }

        /// <summary>
        /// Gets the unique key policy for the repository.
        /// </summary>
        UniqueKeyPolicy DefaultUniqueKeyPolicy { get; }

        /// <summary>
        /// Gets the document client underlying the repository.
        /// </summary>
        /// <returns>A <see cref="Task"/> which, when complete, provides the <see cref="DocumentClient"/>.</returns>
        Task<DocumentClient> GetDocumentClientAsync();

        /// <summary>
        /// Create a document in the store.
        /// </summary>
        /// <typeparam name="T">The type of the item to create.</typeparam>
        /// <param name="item">The item to create.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>The resource response of the created document.</returns>
        Task<ResourceResponse<Document>> CreateAsync<T>(T item, RequestOptions requestOptions = null);

        /// <summary>
        /// Delete a specific document by ID.
        /// </summary>
        /// <param name="id">The document ID.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>A task which completes once the operation is completed.</returns>
        Task DeleteAsync(string id, RequestOptions requestOptions = null);

        /// <summary>
        /// Delete the container related to this repository.
        /// </summary>
        /// <returns>A task which completes when the operation has completed.</returns>
        Task DeleteRepositoryContainerAsync();

        /// <summary>
        /// Execute a query and retrieve a particular page.
        /// </summary>
        /// <typeparam name="T">The type of the items in the result.</typeparam>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        Task<FeedResponse<T>> ExecuteQueryAsync<T>(SqlQuerySpec sqlQuerySpec, int pagesToSkip = 0, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an <see cref="IDocumentQuery{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the items in the result.</typeparam>
        /// <param name="documentQuery">A document query, built from a queryable.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        Task<FeedResponse<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> documentQuery, int pagesToSkip = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a queryable to build a linq query.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="feedOptions">The <see cref="FeedOptions"/>.</param>
        /// <returns>A <see cref="Task"/> which completes with a queryable over the document repository.</returns>
        Task<IOrderedQueryable<T>> CreateDocumentQueryAsync<T>(FeedOptions feedOptions = null);

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the results of the operation.</typeparam>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="func">The function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, Task<TResult>> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the results of the operation.</typeparam>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="func">The asynchronous function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(SqlQuerySpec sqlQuerySpec, Func<T, TResult> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="func">The asynchronous function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        Task ForEachAsync<T>(SqlQuerySpec sqlQuerySpec, Func<T, Task> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Read a document with a given ID.
        /// </summary>
        /// <typeparam name="T">The type of the document to read.</typeparam>
        /// <param name="id">The id of the document to retrieve.</param>
        /// <param name="requestOptions">The request options.</param>
        /// <returns>A task which returns a <see cref="DocumentResponse{TDocument}"/> when the operation completes.</returns>
        Task<DocumentResponse<T>> ReadDocumentAsync<T>(string id, RequestOptions requestOptions = null);

        /// <summary>
        /// Replaces the existing Cosmos DB indexing policy on the repository.
        /// </summary>
        /// <param name="newIndexingPolicy">The new indexing policy.</param>
        /// <param name="progressCallback">A callback for progress.</param>
        /// <param name="requestOptions">Custom Cosmos DB request options for the operation.</param>
        /// <param name="pollIntervalMilliseconds">The poll interval for progress on the reindexing operation.</param>
        /// <param name="cancellationToken">Cancellation token to abandon waiting for the reindexing to complete.</param>
        /// <returns>A task which completes when the indexing operation is complete.</returns>
        /// <exception cref="InvalidOperationException">An indexing operation was already underway. <see cref="WaitForReindexingAsync(Func{long, Task}, int, RequestOptions, CancellationToken)"/> to wait for outstanding operations to complete.</exception>
        /// <remarks>The reindexing operation will always be requested, even if the cancellation token is signalled before the operation has actually started. It will, however, cancel wait for the operation to complete.</remarks>
        Task ReplaceIndexingPolicyAsync(IndexingPolicy newIndexingPolicy = null, Func<long, Task> progressCallback = null, RequestOptions requestOptions = null, int pollIntervalMilliseconds = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the throughput for the container.
        /// </summary>
        /// <param name="rus">The number of RU/s.</param>
        /// <param name="enableRUPerMinute">Whether to enable/disable RU/minute (or leave as is if null).</param>
        /// <returns>A task which completes when the operation has completed.</returns>
        Task SetThroughputAsync(int rus, bool? enableRUPerMinute = null);

        /// <summary>
        /// Gets the currently provisioned throughput for the container.
        /// </summary>
        /// <returns>The throughput in RU/s.</returns>
        Task<int> GetThroughputAsync();

        /// <summary>
        /// Upsert a document into the store.
        /// </summary>
        /// <typeparam name="T">The type of the item to upsert.</typeparam>
        /// <param name="item">The item to upsert.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>The resource response of the created document.</returns>
        /// <remarks>You can use the <see cref="RequestOptions.AccessCondition"/> to manage consistency, or enforce Insert or Update semantics.</remarks>
        Task<ResourceResponse<Document>> UpsertAsync<T>(T item, RequestOptions requestOptions = null);

        /// <summary>
        /// Waits for a reindexing operation to complete.
        /// </summary>
        /// <param name="progressCallback">An optional progress callback.</param>
        /// <param name="pollIntervalMilliseconds">The time to wait in milliseconds between polling the reindexing operation.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <param name="cancellationToken">Cancellation token to abandon the wait.</param>
        /// <returns>A task which completes once the reindexing operation is complete.</returns>
        Task WaitForReindexingAsync(Func<long, Task> progressCallback, int pollIntervalMilliseconds = 1000, RequestOptions requestOptions = null, CancellationToken cancellationToken = default);
    }
}