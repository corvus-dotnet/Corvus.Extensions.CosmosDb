// <copyright file="ConnectionPolicyConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb.Internal
{
    using System;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Serialize / deserialize a <see cref="ConnectionPolicy"/>.
    /// </summary>
    public class ConnectionPolicyConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ConnectionPolicy) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return ConnectionPolicy.Default;
            }

            var jo = JObject.Load(reader);

            var result = new ConnectionPolicy
            {
                ConnectionMode = (ConnectionMode)(int)jo["connectionMode"],
                ConnectionProtocol = (Protocol)(int)jo["connectionProtocol"],
                EnableEndpointDiscovery = (bool)jo["enableEndpointDiscovery"],
                MaxConnectionLimit = (int)jo["maxConnectionLimit"],
                MediaReadMode = (MediaReadMode)(int)jo["mediaReadMode"],
                MediaRequestTimeout = (TimeSpan)jo["mediaRequestTimeout"],
                RequestTimeout = (TimeSpan)jo["requestTimeout"],
                RetryOptions = jo["retryOptions"].ToObject<RetryOptions>(),
            };

            foreach (JToken location in jo["preferredLocations"])
            {
                result.PreferredLocations.Add((string)location);
            }

            string userAgentSuffix = (string)jo["userAgentSuffix"];
            if (!string.IsNullOrEmpty(userAgentSuffix))
            {
                result.UserAgentSuffix = userAgentSuffix;
            }

            return result;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
