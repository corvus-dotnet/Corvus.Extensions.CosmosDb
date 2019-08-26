// <copyright file="Scope.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.GremlinQuery
{
    /// <summary>
    /// Determines how a step will behave in relation to how the traversers are processed.
    /// </summary>
    public enum Scope
    {
        /// <summary>
        /// Informs the step to operate on the entire traversal.
        /// </summary>
        Global,

        /// <summary>
        /// Informs the step to operate on the current object in the step.
        /// </summary>
        Local,
    }
}
