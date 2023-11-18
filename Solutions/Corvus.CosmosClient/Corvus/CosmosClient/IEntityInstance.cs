// <copyright file="IEntityInstance.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient
{
    /// <summary>
    /// An entity decorated with an ETag.
    /// </summary>
    public interface IEntityInstance
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        object Entity { get; set; }

        /// <summary>
        /// Gets or sets the ETag for the entity.
        /// </summary>
        string? ETag { get; set; }
    }

    /// <summary>
    /// An entity decorated with an ETag.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IEntityInstance<T> : IEntityInstance
        where T : notnull
    {
        /// <summary>
        /// Gets or sets the entity of the given type.
        /// </summary>
        new T Entity { get; set; }
    }
}