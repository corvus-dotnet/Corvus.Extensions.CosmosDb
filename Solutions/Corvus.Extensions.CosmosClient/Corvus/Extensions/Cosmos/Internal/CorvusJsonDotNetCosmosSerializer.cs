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
            this.serializer = JsonSerializer.Create(jsonSerializerSettings);
        }

        /// <summary>
        /// Create a serializer that uses the JSON.net serializer.
        /// </summary>
        /// <param name="jsonSerializerSettings">The json serializer settings on which to base the settings.</param>
        /// <param name="cosmosSerializerOptions">The overrides from cosmos serializer options.</param>
        /// <remarks>
        /// This is internal to reduce exposure of JSON.net types so
        /// it is easier to convert to System.Text.Json.
        /// </remarks>
        internal CorvusJsonDotNetCosmosSerializer(JsonSerializerSettings jsonSerializerSettings, CosmosSerializationOptions cosmosSerializerOptions)
        {
            var newSettings = new JsonSerializerSettings()
            {
                CheckAdditionalContent = jsonSerializerSettings.CheckAdditionalContent,
                ConstructorHandling = jsonSerializerSettings.ConstructorHandling,
                Context = jsonSerializerSettings.Context,
                Converters = new List<JsonConverter>(jsonSerializerSettings.Converters),
                Culture = jsonSerializerSettings.Culture,
                DateFormatHandling = jsonSerializerSettings.DateFormatHandling,
                DateFormatString = jsonSerializerSettings.DateFormatString,
                DateParseHandling = jsonSerializerSettings.DateParseHandling,
                DateTimeZoneHandling = jsonSerializerSettings.DateTimeZoneHandling,
                DefaultValueHandling = jsonSerializerSettings.DefaultValueHandling,
                EqualityComparer = jsonSerializerSettings.EqualityComparer,
                Error = jsonSerializerSettings.Error,
                FloatFormatHandling = jsonSerializerSettings.FloatFormatHandling,
                FloatParseHandling = jsonSerializerSettings.FloatParseHandling,
                MaxDepth = jsonSerializerSettings.MaxDepth,
                MetadataPropertyHandling = jsonSerializerSettings.MetadataPropertyHandling,
                MissingMemberHandling = jsonSerializerSettings.MissingMemberHandling,
                ObjectCreationHandling = jsonSerializerSettings.ObjectCreationHandling,
                PreserveReferencesHandling = jsonSerializerSettings.PreserveReferencesHandling,
                ReferenceLoopHandling = jsonSerializerSettings.ReferenceLoopHandling,
                ReferenceResolverProvider = jsonSerializerSettings.ReferenceResolverProvider,
                SerializationBinder = jsonSerializerSettings.SerializationBinder,
                StringEscapeHandling = jsonSerializerSettings.StringEscapeHandling,
                TraceWriter = jsonSerializerSettings.TraceWriter,
                TypeNameAssemblyFormatHandling = jsonSerializerSettings.TypeNameAssemblyFormatHandling,
                TypeNameHandling = jsonSerializerSettings.TypeNameHandling,
                NullValueHandling = cosmosSerializerOptions.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include,
                Formatting = cosmosSerializerOptions.Indented ? Formatting.Indented : Formatting.None,
                ContractResolver = cosmosSerializerOptions.PropertyNamingPolicy == CosmosPropertyNamingPolicy.CamelCase
                    ? new CamelCasePropertyNamesContractResolver()
                    : null,
            };

            this.serializer = JsonSerializer.Create(newSettings);
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