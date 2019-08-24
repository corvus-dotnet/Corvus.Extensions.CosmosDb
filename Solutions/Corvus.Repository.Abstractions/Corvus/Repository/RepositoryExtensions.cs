// <copyright file="RepositoryExtensions.cs" company="Endjin Limited">
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

    /// <summary>
    /// Extensions for the <see cref="IDocumentRepository"/> and related types.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Convert a Cosmos DB <see cref="Document"/> into a specified type.
        /// </summary>
        /// <typeparam name="T">The type to which to convert the document.</typeparam>
        /// <param name="document">The document to convert.</param>
        /// <returns>An instance of the specified type initialied from the Document.</returns>
        public static T As<T>(this Document document)
        {
            return (T)(dynamic)document;
        }

        /// <summary>
        /// Convert a Cosmos DB <see cref="Document"/> into an <see cref="EntityInstance{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="document">The document to convert.</param>
        /// <returns>An <see cref="EntityInstance{T}"/> converted from the specified document.</returns>
        public static EntityInstance<T> AsEntityInstance<T>(this Document document)
        {
            return new EntityInstance<T> { Entity = document.As<T>(), ETag = document.ETag, Timestamp = document.Timestamp };
        }

        /// <summary>
        /// Get a collection of documents as an entity version list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the entities.</typeparam>
        /// <param name="feedResponse">The feedresponse of documents to convert.</param>
        /// <returns>A list of documents representing the specified entities.</returns>
        public static IEnumerable<EntityInstance<T>> AsEntityInstances<T>(this FeedResponse<Document> feedResponse)
        {
            return feedResponse.Select(doc => doc.AsEntityInstance<T>()).ToList();
        }

        /// <summary>
        /// Get a collection of documents as an entity version list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the entities.</typeparam>
        /// <param name="task">The task for the feedresponse of documents to convert.</param>
        /// <returns>A task for a list of documents representing the specified entities.</returns>
        public static async Task<IEnumerable<EntityInstance<T>>> AsEntityInstances<T>(this Task<FeedResponse<Document>> task)
        {
            return (await task.ConfigureAwait(false)).Select(doc => doc.AsEntityInstance<T>()).ToList();
        }

        /// <summary>
        /// Insert a document into the store.
        /// </summary>
        /// <typeparam name="T">The type of the item to insert.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>The resource response of the created document.</returns>
        public static Task<ResourceResponse<Document>> InsertAsync<T>(this IDocumentRepository that, T item, RequestOptions requestOptions = null)
        {
            return that.CreateAsync(item, requestOptions);
        }

        /// <summary>
        /// Update a document in the store.
        /// </summary>
        /// <typeparam name="T">The type of the item to update.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="item">The item to update.</param>
        /// <param name="etag">The current etag of the item.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>The resource response of the created document.</returns>
        public static Task<ResourceResponse<Document>> UpdateAsync<T>(this IDocumentRepository that, T item, string etag, RequestOptions requestOptions = null)
        {
            if (requestOptions == null)
            {
                requestOptions = new RequestOptions();
            }

            requestOptions.AccessCondition = new AccessCondition { Condition = etag, Type = AccessConditionType.IfMatch };
            return that.UpsertAsync(item, requestOptions);
        }

        /// <summary>
        /// Update a document in the store.
        /// </summary>
        /// <typeparam name="T">The type of the item to update.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="entityInstance">The item to update.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>The resource response of the created document.</returns>
        /// <remarks>Note this will use the <see cref="EntityInstance{T}.ETag"/> for the access condition, and the <see cref="EntityInstance{T}.Entity"/> for the new value.</remarks>
        public static Task<ResourceResponse<Document>> UpdateAsync<T>(this IDocumentRepository that, EntityInstance<T> entityInstance, RequestOptions requestOptions = null)
        {
            return that.UpdateAsync(entityInstance.Entity, entityInstance.ETag, requestOptions);
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the specified type, or null if no entity is found.</returns>
        public static async Task<T> GetAsync<T>(this IDocumentRepository that, string id, RequestOptions requestOptions = null)
        {
            DocumentResponse<T> response = await that.ReadDocumentAsync<T>(id, requestOptions).ConfigureAwait(false);
            return response.Document;
        }

        /// <summary>
        /// Delete a specific document by ID.
        /// </summary>
        /// <param name="that">The repository.</param>
        /// <param name="id">The document ID.</param>
        /// <param name="partitionKey">The partition for the document.</param>
        /// <param name="requestOptions">Cosmos DB request options.</param>
        /// <returns>A task which completes once the operation is completed.</returns>
        public static Task DeleteAsync(this IDocumentRepository that, string id, string partitionKey, RequestOptions requestOptions = null)
        {
            return that.DeleteAsync(id, requestOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="partitionKey">The partition for the document.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the specified type, or null if no entity is found.</returns>
        public static async Task<T> GetAsync<T>(this IDocumentRepository that, string id, string partitionKey, RequestOptions requestOptions = null)
        {
            DocumentResponse<T> response = await that.ReadDocumentAsync<T>(id, requestOptions.WithPartitionKeyIfRequired(partitionKey)).ConfigureAwait(false);
            return response.Document;
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the Cosmos DB <see cref="Document"/> type, or null if no entity is found.</returns>
        public static async Task<Document> GetAsync(this IDocumentRepository that, string id, RequestOptions requestOptions = null)
        {
            DocumentResponse<Document> response = await that.ReadDocumentAsync<Document>(id, requestOptions).ConfigureAwait(false);
            return response.Document;
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="partitionKey">The partition key for the entity.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the Cosmos DB <see cref="Document"/> type, or null if no entity is found.</returns>
        public static async Task<Document> GetAsync(this IDocumentRepository that, string id, string partitionKey, RequestOptions requestOptions = null)
        {
            DocumentResponse<Document> response = await that.ReadDocumentAsync<Document>(id, requestOptions.WithPartitionKeyIfRequired(partitionKey)).ConfigureAwait(false);
            return response.Document;
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the specified type, annotated with version metadata.</returns>
        public static async Task<EntityInstance<T>> GetEntityInstanceAsync<T>(this IDocumentRepository that, string id, RequestOptions requestOptions = null)
        {
            DocumentResponse<Document> response = await that.ReadDocumentAsync<Document>(id, requestOptions).ConfigureAwait(false);
            return response.Document.AsEntityInstance<T>();
        }

        /// <summary>
        /// Get an entity from the repository by ID.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the entity.</param>
        /// <param name="partitionKey">The partition key for the entity.</param>
        /// <param name="requestOptions">The Cosmos DB request options.</param>
        /// <returns>An instance of the specified type, annotated with version metadata.</returns>
        public static async Task<EntityInstance<T>> GetEntityInstanceAsync<T>(this IDocumentRepository that, string id, string partitionKey, RequestOptions requestOptions = null)
        {
            DocumentResponse<Document> response = await that.ReadDocumentAsync<Document>(id, requestOptions.WithPartitionKeyIfRequired(partitionKey)).ConfigureAwait(false);
            return response.Document.AsEntityInstance<T>();
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="action">The action to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText), action, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="parameters">The sql parameters for the query.</param>
        /// <param name="action">The action to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText, parameters), action, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="task">The asynchronous task to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, Func<T, Task> task, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText), task, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="parameters">The sql parameters for the query.</param>
        /// <param name="task">The asynchronous task to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, Func<T, Task> task, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText, parameters), task, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="action">The action to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText), action, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="parameters">The sql parameters for the query.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="action">The action to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText, parameters), action, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="task">The asynchronous task to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, Func<T, Task> task, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText), task, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the documents in the repository and carry out an action on each.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="parameters">The sql parameters for the query.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="task">The asynchronous task to execute for each document.</param>
        /// <param name="feedOptions">The feed options for the query.</param>
        /// <param name="cancellationToken">A cancellation token to terminate the enumeration.</param>
        /// <returns>A task which completes when the iteration is complete.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, Func<T, Task> task, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ForEachAsync(new SqlQuerySpec(queryText, parameters), task, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /****/

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the results of the operation.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="func">The function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        public static Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, Func<T, Task<TResult>> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ForEachAsync(sqlQuerySpec, func, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <typeparam name="TResult">The type of the results of the operation.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="func">The asynchronous function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        public static Task<IEnumerable<TResult>> ForEachAsync<T, TResult>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, Func<T, TResult> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ForEachAsync(sqlQuerySpec, func, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, Action<T> action, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ForEachAsync(sqlQuerySpec, action, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Enumerate the entities matching a particular query.
        /// </summary>
        /// <typeparam name="T">The type of entity to enumerate.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="func">The asynchronous function to execute.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task for the async operation.</returns>
        public static Task ForEachAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, Func<T, Task> func, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ForEachAsync(sqlQuerySpec, func, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Read a document with a given ID.
        /// </summary>
        /// <typeparam name="T">The type of the document to read.</typeparam>
        /// <param name="that">The repository.</param>
        /// <param name="id">The id of the document to retrieve.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="requestOptions">The request options.</param>
        /// <returns>A task which returns a <see cref="DocumentResponse{TDocument}"/> when the operation completes.</returns>
        public static Task<DocumentResponse<T>> ReadDocumentAsync<T>(this IDocumentRepository that, string id, string partitionKey, RequestOptions requestOptions = null)
        {
            return that.ReadDocumentAsync<T>(id, requestOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /****/

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), pagesToSkip, feedOptions);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), pagesToSkip, feedOptions);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, 0, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pagesToSkip, feedOptions);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pagesToSkip, feedOptions);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{Document}"/> which can provide more details about the query response.</remarks>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, 0, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pagesToSkip, feedOptions).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pagesToSkip, feedOptions).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, 0, feedOptions, cancellationToken).AsEntityInstances<T>();
        }

        /****/

        /// <summary>
        /// Create a queryable to build a linq query.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="feedOptions">The <see cref="FeedOptions"/>.</param>
        /// <returns>A <see cref="Task"/> which completes with a queryable over the document repository.</returns>
        public static Task<IOrderedQueryable<T>> CreateDocumentQueryAsync<T>(this IDocumentRepository that, string partitionKey, FeedOptions feedOptions = null)
        {
            return that.CreateDocumentQueryAsync<T>(feedOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Execute a query and retrieve a particular page.
        /// </summary>
        /// <typeparam name="T">The type of the items in the result.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, pagesToSkip, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), pagesToSkip, feedOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), pagesToSkip, feedOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<T>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        public static Task<FeedResponse<T>> ExecuteQueryAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<T>(sqlQuerySpec, 0, feedOptions.WithPartitionKeyIfRequired(partitionKey), cancellationToken);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pagesToSkip, feedOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pagesToSkip, feedOptions.WithPartitionKeyIfRequired(partitionKey));
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, string queryText, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) });
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{Document}"/> which can provide more details about the query response.</remarks>
        public static Task<FeedResponse<Document>> ExecuteQueryAsync(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, partitionKey, 0, feedOptions, cancellationToken);
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pageSize, int pageIndex)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), partitionKey, pagesToSkip, feedOptions).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pagesToSkip">The number of pages to skip.</param>
        /// <param name="feedOptions">The Cosmos DB feed options.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int pagesToSkip = 0, FeedOptions feedOptions = null)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), partitionKey, pagesToSkip, feedOptions).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The Cosmos DB SQL Query Specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index to retrieve.</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int pageSize, int pageIndex)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, pageIndex, new FeedOptions { MaxItemCount = pageSize, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="parameters">The SQL query parameters.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, SqlParameterCollection parameters, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText, parameters), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="queryText">The SQL query text.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, string queryText, string partitionKey, int maxItemCount, string continuationToken)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            return that.ExecuteQueryAsync<Document>(new SqlQuerySpec(queryText), new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="maxItemCount">The maximum number of items to retrieve.</param>
        /// <param name="continuationToken">The previous request's continuation token (or null).</param>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, int maxItemCount, string continuationToken)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, new FeedOptions { MaxItemCount = maxItemCount, RequestContinuation = continuationToken, PartitionKey = PartitionKeyIfRequired(partitionKey) }).AsEntityInstances<T>();
        }

        /// <summary>
        /// Execute a query.
        /// </summary>
        /// <typeparam name="T">The type of the item to retrieve.</typeparam>
        /// <param name="that">The Cosmos DB repository.</param>
        /// <param name="sqlQuerySpec">The query specification.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="feedOptions">Cosmos DB feed options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task for an enumerable of objects matching the query specification.</returns>
        /// <remarks>The result is a Cosmos DB <see cref="FeedResponse{T}"/> which can provide more details about the query response.</remarks>
        public static Task<IEnumerable<EntityInstance<T>>> ExecuteQueryForEntityInstancesAsync<T>(this IDocumentRepository that, SqlQuerySpec sqlQuerySpec, string partitionKey, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            return that.ExecuteQueryAsync<Document>(sqlQuerySpec, partitionKey, 0, feedOptions, cancellationToken).AsEntityInstances<T>();
        }

        /// <summary>
        /// Supplies a <see cref="RequestOptions"/> populated with a partition key if necessary.
        /// </summary>
        /// <param name="requestOptions">The existing <see cref="RequestOptions"/>, which may be null.</param>
        /// <param name="partitionKey">The partition key to use, or null if none is required.</param>
        /// <returns>
        /// If a <see cref="RequestOptions"/> is passed in, the same instance will be returned, populated
        /// with the partition key if one was supplied. If <c>requestOptions</c> was null, this will return
        /// null if <c>partitionKey</c> was also null, and otherwise it will return a new <see cref="RequestOptions"/>
        /// populated with the specified partition key.
        /// </returns>
        /// <para>
        /// The purpose of this helper is to enable callers to decide at runtime whether to use a partition
        /// key. This is useful for supporting legacy scenarios in which we need to support collections without
        /// partition keys, in code from which which we also want to be able to use per-DB throughput (which
        /// requires a partition key).
        /// </para>
        private static RequestOptions WithPartitionKeyIfRequired(
            this RequestOptions requestOptions,
            string partitionKey)
        {
            if (requestOptions == null && partitionKey != null)
            {
                requestOptions = new RequestOptions();
            }

            if (partitionKey != null)
            {
                requestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            return requestOptions;
        }

        /// <summary>
        /// Supplies a <see cref="FeedOptions"/> populated with a partition key if necessary.
        /// </summary>
        /// <param name="feedOptions">The existing <see cref="FeedOptions"/>, which may be null.</param>
        /// <param name="partitionKey">The partition key to use, or null if none is required.</param>
        /// <returns>
        /// If a <see cref="FeedOptions"/> is passed in, the same instance will be returned, populated
        /// with the partition key if one was supplied. If <c>feedOptions</c> was null, this will return
        /// null if <c>partitionKey</c> was also null, and otherwise it will return a new <see cref="FeedOptions"/>
        /// populated with the specified partition key.
        /// </returns>
        /// <para>
        /// The purpose of this helper is to enable callers to decide at runtime whether to use a partition
        /// key. This is useful for supporting legacy scenarios in which we need to support collections without
        /// partition keys, in code from which which we also want to be able to use per-DB throughput (which
        /// requires a partition key).
        /// </para>
        private static FeedOptions WithPartitionKeyIfRequired(
            this FeedOptions feedOptions,
            string partitionKey)
        {
            if (feedOptions == null && partitionKey != null)
            {
                feedOptions = new FeedOptions();
            }

            if (partitionKey != null)
            {
                feedOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            return feedOptions;
        }

        /// <summary>
        /// Returns a <see cref="PartitionKey"/> if one is required, and null otherwise.
        /// </summary>
        /// <param name="partitionKey">The partition key value.</param>
        /// <returns>
        /// Null if <c>partitionKey</c> is null. Otherwise, a <see cref="PartitionKey"/> for
        /// the specified <c>partitionKey</c>.
        /// </returns>
        private static PartitionKey PartitionKeyIfRequired(string partitionKey) =>
            partitionKey == null ? null : new PartitionKey(partitionKey);
    }
}
