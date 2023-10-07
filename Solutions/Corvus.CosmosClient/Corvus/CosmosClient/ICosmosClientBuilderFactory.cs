﻿// <copyright file="ICosmosClientBuilderFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient;

using Azure.Core;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

/// <summary>
/// Creates instances of a <see cref="CosmosClient"/>.
/// </summary>
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