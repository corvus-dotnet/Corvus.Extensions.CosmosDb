// <copyright file="CosmosExtensionsServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Extensions.Cosmos;
    using Corvus.Extensions.Cosmos.Internal;
    using Microsoft.Azure.Cosmos;

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
        public static IServiceCollection AddCosmosClientExtensions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ICosmosClientBuilderFactory, CosmosClientBuilderFactory>();
            return serviceCollection;
        }
    }
}
