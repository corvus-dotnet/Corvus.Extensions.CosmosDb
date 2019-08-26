// <copyright file="GremlinClientExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb.GremlinQuery;
    using Corvus.Extensions.CosmosDb.GremlinQuery.Internal;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Gremlin client extension methods.
    /// </summary>
    public static class GremlinClientExtensions
    {
        /// <summary>
        /// Executes a Gremlin query converting the result to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// It is expected that this method will become standard behavior in the Gremlin.NET client in future versions.
        /// </remarks>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="client">This client.</param>
        /// <param name="request">The Gremlin request.</param>
        /// <param name="bindings">The bindings to apply to the query.</param>
        /// <returns>The collection of results.</returns>
        public static async Task<IEnumerable<T>> ExecuteGremlinQueryAsync<T>(this ICosmosDbGremlinClient client, string request, Dictionary<string, object> bindings = null)
        {
            var serializer = JsonSerializer.Create(client.JsonSerializerSettings);
            IReadOnlyCollection<JToken> results = await client.ExecuteGremlinQueryAsync(request, bindings).ConfigureAwait(false);
            return results.SelectMany(o => o).Select(o => BuildJToken(o).ToObject<T>(serializer));
        }

        /// <summary>
        /// Executes a Gremlin query converting the single result to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// It is expected that this method will become standard behavior in the Gremlin.NET client in future versions.
        /// </remarks>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="client">This client.</param>
        /// <param name="request">The Gremlin request.</param>
        /// <param name="bindings">The bindings to apply to the query.</param>
        /// <returns>The collection of results.</returns>
        public static async Task<T> ExecuteGremlinQuerySingleResultAsync<T>(this ICosmosDbGremlinClient client, string request, Dictionary<string, object> bindings = null)
        {
            var serializer = JsonSerializer.Create(client.JsonSerializerSettings);
            JToken result = await client.ExecuteGremlinQuerySingleResultAsync(request, bindings).ConfigureAwait(false);
            return BuildJToken(result).ToObject<T>(serializer);
        }

        /// <summary>
        /// Gets a vertex from the client.
        /// </summary>
        /// <typeparam name="T">The type of the vertex to return.</typeparam>
        /// <param name="client">The client to which to get the vertex.</param>
        /// <param name="id">The id of hte vertex to retrieve.</param>
        /// <returns>A <see cref="Task" /> that completes when the vertex has been retrieved.</returns>
        public static async Task<T> GetVertexAsync<T>(this ICosmosDbGremlinClient client, string id)
        {
            GraphTraversal<dynamic, T> traversal = client.StartTraversal().V<T>(id);
            IEnumerable<T> results = await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Adds a vertex to the client.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex.</typeparam>
        /// <param name="client">The client to which to add the vertex.</param>
        /// <param name="vertex">The object to add as a vertex.</param>
        /// <param name="label">The label of the vertex.</param>
        /// <returns>A <see cref="Task" /> that completes when the vertex has been added.</returns>
        /// <remarks>
        /// If the vertex has a property called <c>label</c>, and no <paramref name="label"/> is suppled then it will be used as the
        /// label for the vertex.
        /// </remarks>
        public static async Task AddVertexAsync<TVertex>(this ICosmosDbGremlinClient client, TVertex vertex, string label = null)
        {
            GraphTraversal<dynamic, TVertex> traversal = client.StartTraversal().AddV(vertex, label);
            await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a traversal against the client.
        /// </summary>
        /// <typeparam name="TStart">The start type of the traversal.</typeparam>
        /// <typeparam name="TEnd">The end type of the traversal.</typeparam>
        /// <param name="client">The client against which to execute the traversal.</param>
        /// <param name="traversal">The traversal.</param>
        /// <returns>The result of the traversal.</returns>
        public static async Task<IEnumerable<TEnd>> ExecuteTraversalAsync<TStart, TEnd>(this ICosmosDbGremlinClient client, ITraversal<TStart, TEnd> traversal)
        {
            ITraversal root = traversal;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            using (var memoryWriter = new MemoryTraversalWriter())
            {
                var bindings = new Dictionary<string, object>();
                await root.WriteAsync(memoryWriter, bindings).ConfigureAwait(false);
                return await client.ExecuteGremlinQueryAsync<TEnd>(memoryWriter.ReadString(), bindings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a traversal against the client with a single result.
        /// </summary>
        /// <typeparam name="TStart">The start type of the traversal.</typeparam>
        /// <typeparam name="TEnd">The end type of the traversal.</typeparam>
        /// <param name="client">The client against which to execute the traversal.</param>
        /// <param name="traversal">The traversal.</param>
        /// <returns>The result of the traversal.</returns>
        public static async Task<TEnd> ExecuteTraversalSingleResultAsync<TStart, TEnd>(this ICosmosDbGremlinClient client, ITraversal<TStart, TEnd> traversal)
        {
            ITraversal root = traversal;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            using (var memoryWriter = new MemoryTraversalWriter())
            {
                var bindings = new Dictionary<string, object>();
                await root.WriteAsync(memoryWriter, bindings).ConfigureAwait(false);
                return await client.ExecuteGremlinQuerySingleResultAsync<TEnd>(memoryWriter.ReadString(), bindings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a vertex to the client.
        /// </summary>
        /// <typeparam name="T">The type of the vertex.</typeparam>
        /// <param name="client">The client containing the vertex to update.</param>
        /// <param name="vertex">The object to update as a vertex.</param>
        /// <returns>A <see cref="Task" /> that completes when the vertex has been added.</returns>
        public static async Task UpdateVertexAsync<T>(this ICosmosDbGremlinClient client, T vertex)
        {
            GraphTraversal<dynamic, dynamic> traversal = client.StartTraversal().Properties(vertex);
            IEnumerable<dynamic> result = await client.ExecuteTraversalAsync(traversal).ConfigureAwait(false);
            if (!result.Any())
            {
                throw new GremlinClientException("Vertex not found", HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Deletes a vertex from the client.
        /// </summary>
        /// <param name="client">The client to which to delete the vertex.</param>
        /// <param name="id">The id of the vertex to drop.</param>
        /// <returns>A <see cref="Task" /> that completes when the edge has been deleted.</returns>
        public static Task DeleteVertexAsync(this ICosmosDbGremlinClient client, string id)
        {
            var bindings = new Dictionary<string, object>
            {
                { "id", id },
            };
            return client.ExecuteGremlinQueryAsync("g.V(id).drop()", bindings);
        }

        /// <summary>
        /// Adds an edge to the client.
        /// </summary>
        /// <param name="client">The client to which to add the edge.</param>
        /// <param name="label">The label of the edge.</param>
        /// <param name="sourceId">The id of the source of the edge.</param>
        /// <param name="targetId">The id of the target of the edge.</param>
        /// <returns>A <see cref="Task" /> that completes when the edge has been added.</returns>
        public static Task AddEdgeAsync(this ICosmosDbGremlinClient client, string label, string sourceId, string targetId)
        {
            GraphTraversal<dynamic, Edge> traversal = client.StartTraversal().V(sourceId).AddE(label).To(targetId);

            return client.ExecuteTraversalAsync(traversal);
        }

        /// <summary>
        /// Deletes an edge from the client.
        /// </summary>
        /// <param name="client">The client to which to add the edge.</param>
        /// <param name="label">The label of the edge.</param>
        /// <param name="sourceId">The id of the source of the edge.</param>
        /// <param name="targetId">The id of the target of the edge.</param>
        /// <returns>A <see cref="Task" /> that completes when the edge has been deleted.</returns>
        public static Task DeleteEdgeAsync(this ICosmosDbGremlinClient client, string label, string sourceId, string targetId)
        {
            var bindings = new Dictionary<string, object>
            {
                { "label", label },
                { "sourceId", sourceId },
                { "targetId", targetId },
            };
            return client.ExecuteGremlinQueryAsync("g.V(sourceId).outE(label).where(inV().has('id', targetId)).drop()", bindings);
        }

        private static JToken BuildJToken(JToken graphSon)
        {
            if (graphSon.Type == JTokenType.Array)
            {
                var array = new JArray();
                foreach (JToken item in (JArray)graphSon)
                {
                    array.Add(BuildJToken(item));
                }

                return array;
            }
            else
            {
                var result = new JObject
                {
                    ["id"] = graphSon["id"],
                    ["label"] = graphSon["label"],
                };

                JToken properties = graphSon["properties"];
                if (properties != null)
                {
                    BuildPropertiesJson(result, properties);
                }

                return result;
            }
        }

        private static void BuildPropertiesJson(JObject result, JToken properties)
        {
            foreach (JProperty property in properties)
            {
                JToken value = property.Value[0]["value"];
                if (value.Type == JTokenType.String)
                {
                    try
                    {
                        var token = JToken.Parse((string)property.Value[0]["value"]);
                        result[property.Name] = token;
                    }
                    catch (JsonReaderException)
                    {
                        result[property.Name] = value;
                    }
                }
                else
                {
                    result[property.Name] = value;
                }
            }
        }
    }
}
