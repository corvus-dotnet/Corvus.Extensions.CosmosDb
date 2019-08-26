// <copyright file="GraphTraversalExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    using System;
    using System.Linq.Expressions;
    using Corvus.Extensions.CosmosDb.GremlinQuery.Internal;

    /// <summary>
    /// Extension methods for the GraphTraversal.
    /// </summary>
    public static class GraphTraversalExtensions
    {
        /// <summary>
        /// Adds an Edge with the specified edge label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="edgeLabel">The edge label.</param>
        /// <returns>A graph traversal for the produced edge.</returns>
        public static GraphTraversal<TStart, Edge> AddE<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string edgeLabel)
        {
            return new AddEdgeGraphTraversal<TStart>(traversal.Client, traversal, edgeLabel);
        }

        /// <summary>
        /// Adds an Edge with the specified edge label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TLabelStart">The start of the traversal that produces the label.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="edgeLabel">A traversal that produces the label for the edge.</param>
        /// <returns>A graph traversal for the produced edge.</returns>
        public static GraphTraversal<TStart, Edge> AddE<TStart, TEnd, TLabelStart>(this GraphTraversal<TStart, TEnd> traversal, ITraversal<TLabelStart, string> edgeLabel)
        {
            return new AddEdgeWithTraversalGraphTraversal<TStart, TLabelStart>(traversal.Client, traversal, edgeLabel);
        }

        /// <summary>
        /// Adds a Vertex with a default vertex label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <returns>A graph traversal for the produced vertex.</returns>
        public static GraphTraversal<TStart, Vertex> AddV<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal)
        {
            return new AddVertexGraphTraversal<TStart, TEnd, Vertex>(traversal.Client, traversal);
        }

        /// <summary>
        /// Adds a Vertex with the specified vertex label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexLabel">The label for the vertex.</param>
        /// <returns>A graph traversal for the produced vertex.</returns>
        public static GraphTraversal<TStart, Vertex> AddV<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string vertexLabel)
        {
            return new AddVertexGraphTraversal<TStart, TEnd, Vertex>(traversal.Client, traversal, vertexLabel);
        }

        /// <summary>
        /// Adds a Vertex with the specified vertex label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TVertex">The type of the vertex.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertex">The vertex to add.</param>
        /// <param name="vertexLabel">The label for the vertex.</param>
        /// <returns>A graph traversal for the produced vertex.</returns>
        public static GraphTraversal<TStart, TVertex> AddV<TStart, TEnd, TVertex>(this GraphTraversal<TStart, TEnd> traversal, TVertex vertex, string vertexLabel)
        {
            return new AddVertexGraphTraversal<TStart, TEnd, TVertex>(traversal.Client, traversal, vertexLabel, vertex);
        }

        /// <summary>
        /// Adds a Vertex with the specified vertex label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TLabelStart">The start of the traversal that produces the label.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexLabel">A traversal that produces the label for the vertex.</param>
        /// <returns>A graph traversal for the produced edge.</returns>
        public static GraphTraversal<TStart, TEnd> AddV<TStart, TEnd, TLabelStart>(this GraphTraversal<TStart, TEnd> traversal, ITraversal<TLabelStart, string> vertexLabel)
        {
            return new AddVertexWithTraversalGraphTraversal<TStart, TEnd, TLabelStart>(traversal.Client, traversal, vertexLabel);
        }

        /// <summary>
        /// Adds a Vertex with the specified vertex label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the endt of the traversal.</typeparam>
        /// <typeparam name="TLabelStart">The start of the traversal that produces the label.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertex">The vertex to add.</param>
        /// <param name="vertexLabel">A traversal that produces the label for the vertex.</param>
        /// <returns>A graph traversal for the produced edge.</returns>
        public static GraphTraversal<TStart, TEnd> AddV<TStart, TEnd, TLabelStart>(this GraphTraversal<TStart, TEnd> traversal, TEnd vertex, ITraversal<TLabelStart, string> vertexLabel)
        {
            return new AddVertexWithTraversalGraphTraversal<TStart, TEnd, TLabelStart>(traversal.Client, traversal, vertexLabel, vertex);
        }

        /// <summary>
        /// Updates the properties of a Vertex.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertex">The vertex to update.</param>
        /// <returns>A graph traversal for the updated vertex.</returns>
        public static GraphTraversal<TStart, TEnd> Properties<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, TEnd vertex)
        {
            return new UpdateVertexGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertex);
        }

        /// <summary>
        /// Ensures that all of the provided traversals yield a result.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="andTraversals">The traversals that must yield a result.</param>
        /// <returns>The And traversal. </returns>
        public static GraphTraversal<TStart, TEnd> And<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params ITraversal[] andTraversals)
        {
            return new AndGraphTraversal<TStart, TEnd>(traversal.Client, traversal, andTraversals);
        }

        /// <summary>
        /// A step modulator that provides a label to the step that can be accessed later in the traversal by other steps.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="stepLabel">The step label.</param>
        /// <param name="stepLabels">Additional step labels.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> As<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string stepLabel, params string[] stepLabels)
        {
            return new AsGraphTraversal<TStart, TEnd>(traversal.Client, traversal, stepLabel, stepLabels);
        }

        /// <summary>
        ///  A step modulator used with Group and Order.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> By<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal)
        {
            return new ByGraphTraversal<TStart, TEnd>(traversal.Client, traversal);
        }

        /// <summary>
        /// A step modulator that provides a label to the step that can be accessed later in the traversal by other steps.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="key">The key to use for comparison.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> By<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string key)
        {
            return new ByGraphTraversal<TStart, TEnd>(traversal.Client, traversal, key);
        }

        /// <summary>
        /// A step modulator that provides a label to the step that can be accessed later in the traversal by other steps.
        /// </summary>
        /// <typeparam name="TStart">The start type of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the vertex.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="key">The key to use for comparison.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> By<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, Expression<Func<TEnd, string>> key)
        {
            return new ByGraphTraversal<TStart, TEnd>(traversal.Client, traversal, key);
        }

        /// <summary>
        /// Evaluates the provided traversals and returns the result of the first traversal to emit at least one object.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="traversals">The traversals to coalesce.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Coalesce<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params ITraversal<TEnd>[] traversals)
        {
            return new CoalesceGraphTraversal<TStart, TEnd>(traversal.Client, traversal, traversals);
        }

        /// <summary>
        /// Map the traversal stream to its reduction as a sum of the Traverser.bulk() values given the specified Scope (i.e. count the number of traversers up to this point).
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="scope">The scope of the traversal.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, long> Count<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, Scope? scope = null)
        {
            return new CountGraphTraversal<TStart, long>(traversal.Client, traversal, scope);
        }

        /// <summary>
        /// Map the traversal stream to its reduction as a sum of the Traverser.bulk() values given the specified Scope (i.e. count the number of traversers up to this point).
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="dedupLabels">if labels are provided, then the scoped object's labels determine de-duplication. No labels implies current object.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Dedup<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params string[] dedupLabels)
        {
            return new DedupGraphTraversal<TStart, TEnd>(traversal.Client, traversal, dedupLabels);
        }

        /// <summary>
        /// Map the traversal stream to its reduction as a sum of the Traverser.bulk() values given the specified Scope (i.e. count the number of traversers up to this point).
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Drop<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal)
        {
            return new DropGraphTraversal<TStart, TEnd>(traversal.Client, traversal);
        }

        /// <summary>
        /// Rolls up objects in the stream into an aggregate list.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Fold<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal)
        {
            return new FoldToListGraphTraversal<TStart, TEnd>(traversal.Client, traversal);
        }

        /// <summary>
        /// Rolls up objects in the stream into an aggregate list.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TEnd2">The end type of the folded traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="seed">The seed for the fold function.</param>
        /// <param name="foldFunction">The fold fucntion.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd2> Fold<TStart, TEnd, TEnd2>(this GraphTraversal<TStart, TEnd> traversal, TEnd2 seed, string foldFunction)
        {
            return new FoldGraphTraversal<TStart, TEnd2>(traversal.Client, traversal, seed, foldFunction);
        }

        /// <summary>
        /// Filters vertices, edges and vertex properties based on the existence of properties.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The parent traversal.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Has<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string propertyKey)
        {
            return new HasGraphTraversal<TStart, TEnd>(traversal.Client, traversal, propertyKey);
        }

        /// <summary>
        /// Filters vertices, edges and vertex properties based on the existence of properties.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The parent traversal.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="predicate">The property predicate.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Has<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string propertyKey, string predicate)
        {
            return new HasPropertyPredicateGraphTraversal<TStart, TEnd>(traversal.Client, traversal, propertyKey, predicate);
        }

        /// <summary>
        /// Filters vertices, edges and vertex properties based on the existence of properties.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TValue">The type of the property value to test.</typeparam>
        /// <param name="traversal">The parent traversal.</param>
        /// <param name="propertyKey">The property key.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> Has<TStart, TEnd, TValue>(this GraphTraversal<TStart, TEnd> traversal, string propertyKey, TValue propertyValue)
        {
            return new HasPropertyValueGraphTraversal<TStart, TEnd, TValue>(traversal.Client, traversal, propertyKey, propertyValue);
        }

        /// <summary>
        /// Filters vertices, edges and vertex properties based on the label.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The parent traversal.</param>
        /// <param name="label">The label.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> HasLabel<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string label)
        {
            return new HasLabelGraphTraversal<TStart, TEnd>(traversal.Client, traversal, label);
        }

        /// <summary>
        /// Map the Vertex to its incoming adjacent vertices given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, Vertex> In<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new InGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its incoming adjacent vertices given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TVertex">The type of the target vertex.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TVertex> In<TStart, TEnd, TVertex>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new InGraphTraversal<TStart, TEnd, TVertex>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its incoming incident edges given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, Edge> InE<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new InEGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its incoming incident edges given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TEdge">The type of the edge.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEdge> InE<TStart, TEnd, TEdge>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new InEGraphTraversal<TStart, TEnd, TEdge>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its outgoing adjacent vertices given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, Vertex> Out<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new OutGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its outgoing adjacent vertices given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TVertex">The type of the target vertex.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TVertex> Out<TStart, TEnd, TVertex>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new OutGraphTraversal<TStart, TEnd, TVertex>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its outgoing incident edges given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, Edge> OutE<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new OutEGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// Map the Vertex to its outgoing incident edges given the edge labels.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <typeparam name="TEdge">The type of the edge.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexIds">The target vertex ids.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEdge> OutE<TStart, TEnd, TEdge>(this GraphTraversal<TStart, TEnd> traversal, params string[] vertexIds)
        {
            return new OutEGraphTraversal<TStart, TEnd, TEdge>(traversal.Client, traversal, vertexIds);
        }

        /// <summary>
        /// When used as a modifier to addE(String) this method specifies the traversal to use for selecting the incoming vertex of the newly added Edge.
        /// </summary>
        /// <typeparam name="TStart">The type of the start of the traversal.</typeparam>
        /// <typeparam name="TEnd">The type of the end of the traversal.</typeparam>
        /// <param name="traversal">The traversal for which this is an extension.</param>
        /// <param name="vertexId">The target vertex.</param>
        /// <returns>The traversal.</returns>
        public static GraphTraversal<TStart, TEnd> To<TStart, TEnd>(this GraphTraversal<TStart, TEnd> traversal, string vertexId)
        {
            return new ToGraphTraversal<TStart, TEnd>(traversal.Client, traversal, vertexId);
        }
    }
}
