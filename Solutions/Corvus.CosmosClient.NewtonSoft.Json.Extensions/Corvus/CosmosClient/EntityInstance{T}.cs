// <copyright file="EntityInstance{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos
{
    using System;
    using System.Collections.Generic;

    using Corvus.CosmosClient;
    using Corvus.Extensions.Cosmos.Internal;

    using Newtonsoft.Json;

    /// <summary>
    /// An instance of an entity from a Cosmos client.
    /// </summary>
    /// <typeparam name="T">Type type of the entity.</typeparam>
    /// <remarks>This provides a serializable version of the response which decorates the entity with an <see cref="ETag"/>.</remarks>
    [JsonConverter(typeof(EntityInstanceJsonConverter))]
    public sealed class EntityInstance<T> : IEquatable<IEntityInstance<T>>, IEntityInstance<T>
        where T : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstance{T}"/> struct.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="eTag">The etag.</param>
        public EntityInstance(T entity, string? eTag)
        {
            this.Entity = entity;
            this.ETag = eTag;
        }

        /// <summary>
        /// Gets or sets the ETag for the instance.
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        public T Entity { get; set; }

        /// <inheritdoc/>
        object IEntityInstance.Entity
        {
            get => this.Entity;
            set => this.Entity = (T)value;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">The lhs for comparison.</param>
        /// <param name="right">The rhs for comparison.</param>
        /// <returns>True if the instances compare for equality using the default <see cref="EqualityComparer{T}"/> and they also share a common ETag.</returns>
        public static bool operator ==(EntityInstance<T> left, EntityInstance<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">The lhs for comparison.</param>
        /// <param name="right">The rhs for comparison.</param>
        /// <returns>False if the instances compare for equality using the default <see cref="EqualityComparer{T}"/> and they also share a common ETag.</returns>
        public static bool operator !=(EntityInstance<T> left, EntityInstance<T> right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is IEntityInstance<T> instance && this.Equals(instance);
        }

        /// <inheritdoc/>
        public bool Equals(IEntityInstance<T>? other)
        {
            return other is not null && this.ETag == other.ETag &&
                   EqualityComparer<T>.Default.Equals(this.Entity, other.Entity);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ETag, this.Entity);
        }
    }
}