// <copyright file="CorvusJsonDotNetCosmosSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// <derivation>
// Derived from material Copyright (c) Microsoft Corporation.  All rights reserved.
// https://raw.githubusercontent.com/Azure/azure-cosmos-dotnet-v3/0843cae3c252dd49aa8e392623d7eaaed7eb712b/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonDotNetSerializer.cs
// </derivation>

namespace Corvus.Extensions.Cosmos.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The default Cosmos JSON.NET serializer.
    /// </summary>
    internal sealed class CorvusJsonDotNetCosmosSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializer serializer;

        /// <summary>
        /// Create a serializer that uses the JSON.net serializer.
        /// </summary>
        /// <param name="jsonSerializerSettings">The Json serializer settings.</param>
        /// <remarks>
        /// This is internal to reduce exposure of JSON.net types so
        /// it is easier to convert to System.Text.Json.
        /// </remarks>
        internal CorvusJsonDotNetCosmosSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            if (jsonSerializerSettings is null)
            {
                throw new System.ArgumentNullException(nameof(jsonSerializerSettings));
            }

            this.serializer = JsonSerializer.Create(jsonSerializerSettings);
        }

        /// <summary>
        /// Convert a Stream to the passed in type.
        /// </summary>
        /// <typeparam name="T">The type of object that should be deserialized.</typeparam>
        /// <param name="stream">An open stream that is readable that contains JSON.</param>
        /// <returns>The object representing the deserialized stream.</returns>
        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return this.serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        /// <summary>
        /// Converts an object to a open readable stream.
        /// </summary>
        /// <typeparam name="T">The type of object being serialized.</typeparam>
        /// <param name="input">The object to be serialized.</param>
        /// <returns>An open readable stream containing the JSON of the serialized object.</returns>
        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.None;
                    this.serializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}