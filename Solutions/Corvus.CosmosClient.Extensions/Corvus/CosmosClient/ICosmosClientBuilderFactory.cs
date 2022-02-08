// <copyright file="ICosmosClientBuilderFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient;

using Azure.Core;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

/// <summary>
/// Creates instances of a <see cref="CosmosClient"/>.
/// </summary>
/// <remarks>
/// TODO: is this the right project for this? I don't like the way anything just wanting this factory interface
/// ends up dragging in Corvus.Retry because it happens to share a library with a bunch of ForEachAsync extension
/// methods for Container.
/// Should this just be in a library called Corvus.CosmosClient? (And Corvus.CosmosClient.NewtonSoft.Json.Extensions
/// should just be Corvus.CosmosClient.NewtonSoft.Json, since it has no extension methods.)
/// </remarks>
public interface ICosmosClientBuilderFactory
{
    /// <summary>
    /// Create a Cosmos Client Builder.
    /// </summary>
    /// <param name="accountEndpoint">The account endpoint for the client.</param>
    /// <param name="accountKey">The account key for the client.</param>
    /// <returns>A <see cref="CosmosClientBuilder"/> preconfigured for the environment.</returns>
    CosmosClientBuilder CreateCosmosClientBuilder(string accountEndpoint, string accountKey);

    /// <summary>
    /// Create a Cosmos Client Builder.
    /// </summary>
    /// <param name="accountEndpoint">The account endpoint for the client.</param>
    /// <param name="tokenCredential">The credentials the client should use.</param>
    /// <returns>A <see cref="CosmosClientBuilder"/> preconfigured for the environment.</returns>
    CosmosClientBuilder CreateCosmosClientBuilder(string accountEndpoint, TokenCredential tokenCredential);

    /// <summary>
    /// Create a Cosmos Client Builder.
    /// </summary>
    /// <param name="connectionString">The connection string for the client.</param>
    /// <returns>A <see cref="CosmosClientBuilder"/> preconfigured for the environment.</returns>
    CosmosClientBuilder CreateCosmosClientBuilder(string connectionString);
}