// <copyright file="ITraversal{TStart,TEnd}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// A Gremlin traversal.
    /// </summary>
    /// <typeparam name="TStart">The start of the traversal.</typeparam>
    /// <typeparam name="TEnd">The end of the traversal.</typeparam>
    public interface ITraversal<TStart, TEnd> : ITraversal<TEnd>
    {
    }
}
