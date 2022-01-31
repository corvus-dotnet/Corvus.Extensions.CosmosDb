// <copyright file="CosmosExtensionsServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.Cosmos.Internal;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Extensions to install the services needed for the Cosmos client extensions.
    /// </summary>
    public static class CosmosExtensionsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services for the <see cref="CosmosClient"/> extensions.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the services should be added.</param>
        /// <returns>The service collection configured with the services.</returns>
        /// <remarks>
        /// <para>
        /// Also adds the Corvus.Extensions.Newtonsoft.Json <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/>
        /// and the four JsonConverters provided by that library to the collection (if not already present).
        /// </para>
        /// <para>
        /// Version 2 of Corvus.Extensions.Newtonsoft.Json provides separate methods to register each of the JsonConverters
        /// it provides. As such, this method is replaced with <see cref="AddCosmosClientBuilder"/>; calling code
        /// should switch to that method and register any required JsonConverters separately.
        /// </para>
        /// </remarks>
        [Obsolete("Use AddCosmosClientBuilder and register JsonConverters separately (see remarks).")]
        public static IServiceCollection AddCosmosClientExtensions(this IServiceCollection serviceCollection)
        {
            if (serviceCollection.Any(s => typeof(ICosmosClientBuilderFactory).IsAssignableFrom(s.ServiceType)))
            {
                return serviceCollection;
            }

            serviceCollection.AddJsonNetSerializerSettingsProvider();
            serviceCollection.AddJsonNetPropertyBag();
            serviceCollection.AddJsonNetCultureInfoConverter();
            serviceCollection.AddJsonNetDateTimeOffsetToIso8601AndUnixTimeConverter();
            serviceCollection.AddSingleton<JsonConverter>(new StringEnumConverter(true));

            serviceCollection.AddSingleton<ICosmosClientBuilderFactory, CosmosClientBuilderFactory>();
            return serviceCollection;
        }

        /// <summary>
        /// Adds the service <see cref="ICosmosClientBuilderFactory"/> to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which the services should be added.</param>
        /// <returns>The service collection configured with the services.</returns>
        /// <remarks>
        /// Also adds the Corvus.Extensions.Newtonsoft.Json <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/>
        /// to the collection (if not already present).
        /// </remarks>
        public static IServiceCollection AddCosmosClientBuilder(this IServiceCollection serviceCollection)
        {
            if (serviceCollection.Any(s => typeof(ICosmosClientBuilderFactory).IsAssignableFrom(s.ServiceType)))
            {
                return serviceCollection;
            }

            serviceCollection.AddJsonNetSerializerSettingsProvider();
            serviceCollection.AddSingleton<ICosmosClientBuilderFactory, CosmosClientBuilderFactory>();
            return serviceCollection;
        }
    }
}