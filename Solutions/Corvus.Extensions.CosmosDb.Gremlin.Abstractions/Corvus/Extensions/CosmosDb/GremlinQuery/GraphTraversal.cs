// <copyright file="GraphTraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.Extensions.CosmosDb.GremlinQuery.Internal;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A gremlin graph traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    /// <typeparam name="TEnd">The end of the traversal.</typeparam>
    public abstract class GraphTraversal<TStart, TEnd> : ITraversal<TStart, TEnd>
    {
        private readonly List<IStep> steps = new List<IStep>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTraversal{TStart, TEnd}"/> class.
        /// </summary>
        /// <param name="client">The Gremlin client for which this is a traversal.</param>
        /// <param name="parent">The parent of this traversal.</param>
        protected GraphTraversal(ICosmosDbGremlinClient client, ITraversal parent)
        {
            this.Client = client;
            this.Parent = parent;
            parent?.Add(this);
        }

        /// <inheritdoc/>
        public ITraversal Parent { get; }

        /// <summary>
        /// Gets the client for which this is a traversal.
        /// </summary>
        public ICosmosDbGremlinClient Client { get; }

        /// <inheritdoc/>
        public void Add(IStep child)
        {
            this.steps.Add(child);
        }

        /// <inheritdoc/>
        public virtual async Task WriteAsync(ITraversalWriter writer, Dictionary<string, object> bindings)
        {
            foreach (IStep child in this.steps)
            {
                await child.WriteAsync(writer, bindings).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets a vertex or vertices, usually at the start of a graph traversal.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex to get.</typeparam>
        /// <param name="vertexIds">The vertex Ids.</param>
        /// <returns>A graph. </returns>
        public GraphTraversal<TStart, TVertex> V<TVertex>(params string[] vertexIds)
        {
            // This is not implemented as an extension method because we have to supply the TEnd
            // type parameter - it cannot be inferred from the method arguments. This would mean we
            // would also have to supply TStart and it would become very unpleasant.
            return new GetVertexGraphTraversal<TStart, TVertex>(this.Client, this, vertexIds);
        }

        /// <summary>
        /// Gets a vertex or vertices, usually at the start of a graph traversal.
        /// </summary>
        /// <param name="vertexIds">The vertex Ids.</param>
        /// <returns>A graph. </returns>
        public GraphTraversal<TStart, Vertex> V(params string[] vertexIds)
        {
            // This is not implemented as an extension method because we have to supply the TEnd
            // type parameter - it cannot be inferred from the method arguments. This would mean we
            // would also have to supply TStart and it would become very unpleasant.
            return new GetVertexGraphTraversal<TStart, Vertex>(this.Client, this, vertexIds);
        }

        /// <summary>
        /// Get a unique binding name for a given binding dictionary.
        /// </summary>
        /// <param name="bindings">The dictionary of bindings.</param>
        /// <returns>A unique name for the binding dictionary.</returns>
        protected static string GetBindingName(Dictionary<string, object> bindings)
        {
            return "binding" + (bindings.Count + 1);
        }

        /// <summary>
        /// Write a bound value to the traversal.
        /// </summary>
        /// <typeparam name="T">The type of the object to add.</typeparam>
        /// <param name="writer">The writer to use.</param>
        /// <param name="bindings">The dictionary of bindings.</param>
        /// <param name="value">The value of a binding.</param>
        /// <returns>A unique name for the binding dictionary.</returns>
        protected static Task WriteBoundValueAsync<T>(ITraversalWriter writer, Dictionary<string, object> bindings, T value)
        {
            string key = GetBindingName(bindings);
            bindings.Add(key, value);
            return writer.WriteAsync(key);
        }

        /// <summary>
        /// Writes entity properties, and returns the value of the ID property if found.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to write.</param>
        /// <param name="writer">The writer to which to write the vertex.</param>
        /// <param name="bindings">The bindings collection for the traversal.</param>
        /// <param name="includeIdAndPartitionKey">
        /// Indicates whether to include the id and partition key. When creating a new vertex, this should be
        /// true, to provide the new value, but it should be false when updating a vertex, since it is
        /// illegal to modify an existing vertex's id or partition key (and even if you attempt to set these
        /// to the value they already has, CosmosDB will complain).</param>
        /// <returns>The value of the ID property, if it was found.</returns>
        protected async Task<(string id, string partitionKey)> WriteProperties<TEntity>(
            TEntity entity,
            ITraversalWriter writer,
            Dictionary<string, object> bindings,
            bool includeIdAndPartitionKey)
        {
            string id = string.Empty;
            string partitionKey = string.Empty;
            if (!EqualityComparer<TEntity>.Default.Equals(entity, default))
            {
                var serializer = JsonSerializer.Create(this.Client.JsonSerializerSettings);
                var serializedObject = JObject.FromObject(entity, serializer);
                foreach (JProperty prop in serializedObject.Properties())
                {
                    bool isPartitionKey = this.Client?.PartitionKeyDefinition?.Paths?.Count > 0 && this.Client.PartitionKeyDefinition.Paths.Contains("/" + prop.Name);
                    bool isId = prop.Name == "id";

                    if (prop.Name != "label" && (includeIdAndPartitionKey || (!isPartitionKey && !isId)))
                    {
                        string bindingName = GetBindingName(bindings);
                        bindings.Add(bindingName, prop.Value.ToString());
                        await writer.WriteAsync(".property('").ConfigureAwait(false);
                        await writer.WriteAsync(prop.Name).ConfigureAwait(false);
                        await writer.WriteAsync("',").ConfigureAwait(false);
                        await writer.WriteAsync(bindingName).ConfigureAwait(false);
                        await writer.WriteAsync(")").ConfigureAwait(false);
                    }

                    if (isId)
                    {
                        id = prop.Value.ToString();
                    }

                    if (isPartitionKey)
                    {
                        partitionKey = prop.Value.ToString();
                    }
                }
            }

            return (id, partitionKey);
        }
    }
}
