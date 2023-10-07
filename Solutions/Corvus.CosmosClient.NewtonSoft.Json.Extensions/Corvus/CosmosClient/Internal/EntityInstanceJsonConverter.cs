﻿// <copyright file="EntityInstanceJsonConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos.Internal
{
    using System;

    using Corvus.CosmosClient;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A type converter which can read an <see cref="EntityInstance{T}"/>
    /// from a serialized instance of type T in a CosmosDB document.
    /// </summary>
    internal class EntityInstanceJsonConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(EntityInstance<>);
        }

        /// <inheritdoc/>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(serializer);

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jo = JObject.Load(reader);

            string etag = (string)jo["_etag"]!;
            object entity = serializer.Deserialize(jo.CreateReader(), objectType.GetGenericArguments()[0])!;

            // We are certain that the object is of type EntityInstance<> at this point
            // so we can instantiate it using the (Entity, ETag) constructor.
            object instance = Activator.CreateInstance(objectType, entity, etag)!;

            return instance;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(serializer);

            if (value is not IEntityInstance entityInstance)
            {
                throw new ArgumentException("Value is not an IEntityInstance", nameof(value));
            }

            var jObject = JObject.FromObject(entityInstance.Entity, serializer);
            jObject["_etag"] = entityInstance.ETag;
            jObject.WriteTo(writer);
        }
    }
}