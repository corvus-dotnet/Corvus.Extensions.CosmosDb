// <copyright file="ICosmosOptionsFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Creates instances of a <see cref="CosmosClient"/>.
/// </summary>
/// <remarks>
/// <para>
/// There are two ways to configure client options (such as retries or serializer settings) for
/// the <see cref="CosmosClient"/>. You can build a <see cref="CosmosClientOptions"/> object,
/// which this interface supports. This will populate such an object with whatever settings have
/// been ambiently configured for the application. (A common use case is to set up JSON
/// serialization to support whatever customer serialization the application requires.)
/// The alternative is the fluent API supported by <see cref="ICosmosClientBuilderFactory"/>.
/// </para>
/// </remarks>
public interface ICosmosOptionsFactory
{
    /// <summary>
    /// Create Cosmos Client options.
    /// </summary>
    /// <returns>A <see cref="CosmosClientOptions"/> preconfigured for the environment.</returns>
    CosmosClientOptions CreateCosmosClientOptions();
}