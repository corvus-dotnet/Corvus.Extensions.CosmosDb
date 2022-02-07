// <copyright file="CosmosClientNewtonsoftJsonServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection;

using System.Linq;

using Corvus.CosmosClient;
using Corvus.CosmosClient.Internal;
using Corvus.Extensions.Json;

using Newtonsoft.Json;

/// <summary>
/// Extensions to install the services needed for customizing Newtonsoft.Json-base serialization
/// in the Cosmos client.
/// </summary>
public static class CosmosClientNewtonsoftJsonServiceCollectionExtensions
{
    /// <summary>
    /// Adds the service <see cref="ICosmosClientBuilderFactory"/> to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services should be added.</param>
    /// <returns>The service collection configured with the services.</returns>
    /// <remarks>
    /// Also adds the Corvus.Extensions.Newtonsoft.Json <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/>
    /// to the collection (if not already present).
    /// </remarks>
    public static IServiceCollection AddCosmosClientBuilderWithNewtonsoftJsonIntegration(
        this IServiceCollection serviceCollection)
    {
        if (serviceCollection.IsCosmosClientBuilderAlreadyRegistered())
        {
            return serviceCollection;
        }

        serviceCollection.AddJsonNetSerializerSettingsProvider();
        serviceCollection.AddSingleton<CorvusJsonCosmosClientBuilderFactory>();
        serviceCollection.AddSingleton<ICosmosClientBuilderFactory>(sp => sp.GetRequiredService<CorvusJsonCosmosClientBuilderFactory>());
        serviceCollection.AddSingleton<ICosmosOptionsFactory>(sp => sp.GetRequiredService<CorvusJsonCosmosClientBuilderFactory>());

        return serviceCollection;
    }

    /// <summary>
    /// Adds the service <see cref="ICosmosClientBuilderFactory"/> to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services should be added.</param>
    /// <param name="serializerSettingsProvider">
    /// The provider from which to get the serialization settings to use with Comos DB operations.</param>
    /// <returns>The service collection configured with the services.</returns>
    /// <remarks>
    /// Also adds the Corvus.Extensions.Newtonsoft.Json <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/>
    /// to the collection (if not already present).
    /// </remarks>
    public static IServiceCollection AddCosmosClientBuilderWithNewtonsoftJsonIntegration(
        this IServiceCollection serviceCollection,
        IJsonSerializerSettingsProvider serializerSettingsProvider)
    {
        if (serviceCollection.IsCosmosClientBuilderAlreadyRegistered())
        {
            return serviceCollection;
        }

        return serviceCollection.AddCosmosClientBuilder(new CorvusJsonCosmosClientBuilderFactory(serializerSettingsProvider));
    }

    /// <summary>
    /// Adds the service <see cref="ICosmosClientBuilderFactory"/> to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services should be added.</param>
    /// <param name="serializerSettings">The serialization settings to use with Comos DB operations.</param>
    /// <returns>The service collection configured with the services.</returns>
    /// <remarks>
    /// Also adds the Corvus.Extensions.Newtonsoft.Json <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/>
    /// to the collection (if not already present).
    /// </remarks>
    public static IServiceCollection AddCosmosClientBuilderWithNewtonsoftJsonIntegration(
        this IServiceCollection serviceCollection,
        JsonSerializerSettings serializerSettings)
    {
        if (serviceCollection.IsCosmosClientBuilderAlreadyRegistered())
        {
            return serviceCollection;
        }

        return serviceCollection.AddCosmosClientBuilder(CorvusJsonCosmosClientBuilderFactory.FromSettings(serializerSettings));
    }

    private static bool IsCosmosClientBuilderAlreadyRegistered(this IServiceCollection serviceCollection)
    {
        return serviceCollection.Any(s => typeof(ICosmosClientBuilderFactory).IsAssignableFrom(s.ServiceType));
    }

    private static IServiceCollection AddCosmosClientBuilder(
        this IServiceCollection serviceCollection,
        CorvusJsonCosmosClientBuilderFactory factory)
    {
        serviceCollection.AddJsonNetSerializerSettingsProvider();
        serviceCollection.AddSingleton<ICosmosClientBuilderFactory>(factory);
        serviceCollection.AddSingleton<ICosmosOptionsFactory>(factory);
        return serviceCollection;
    }
}