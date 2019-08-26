// <copyright file="ITraversal.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// A traversal of unknown source/destination type.
    /// </summary>
    public interface ITraversal : IStep
    {
        /// <summary>
        /// Gets the parent for this traversal.
        /// </summary>
        ITraversal Parent { get; }

        /// <summary>
        /// Add a child step to the parent.
        /// </summary>
        /// <param name="child">The child to add.</param>
        void Add(IStep child);
    }
}
