// <copyright file="EntityInstance{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Repository
{
    using System;

    /// <summary>
    /// An instance of an entity from a <see cref="IDocumentRepository"/>.
    /// </summary>
    /// <typeparam name="T">Type type of the entity.</typeparam>
    public class EntityInstance<T>
    {
        /// <summary>
        /// Gets or sets the ETag for the instance.
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the instance.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        public T Entity { get; set; }
    }
}
