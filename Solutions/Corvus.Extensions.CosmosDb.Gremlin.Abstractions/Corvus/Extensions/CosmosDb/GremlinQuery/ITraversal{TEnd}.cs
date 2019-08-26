// <copyright file="ITraversal{TEnd}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// A traversal which produces a specific result.
    /// </summary>
    /// <typeparam name="TEnd">The type of the results of the traversal.</typeparam>
    public interface ITraversal<TEnd> : ITraversal
    {
    }
}
