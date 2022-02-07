// <copyright file="CorvusJsonCosmosClientBuilderFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Internal;

using System;

using Azure.Core;

using Corvus.CosmosClient;
using Corvus.Extensions.Json;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

using Newtonsoft.Json;

/// <summary>
/// A factory which can create a <see cref="CosmosClientBuilder"/>.
/// </summary>
internal class CorvusJsonCosmosClientBuilderFactory : ICosmosClientBuilderFactory, ICosmosOptionsFactory
{
    private readonly IJsonSerializerSettingsProvider? serializerSettingsProvider;
    private readonly JsonSerializerSettings? settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorvusJsonCosmosClientBuilderFactory"/> class.
    /// </summary>
    /// <param name="serializerSettingsProvider">The <see cref="IJsonSerializerSettingsProvider"/> for the environment.</param>
    public CorvusJsonCosmosClientBuilderFactory(IJsonSerializerSettingsProvider serializerSettingsProvider)
    {
        ArgumentNullException.ThrowIfNull(serializerSettingsProvider);
        this.serializerSettingsProvider = serializerSettingsProvider;
    }

    private CorvusJsonCosmosClientBuilderFactory(JsonSerializerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        this.settings = settings;
    }

    /// <summary>
    /// Creates a <see cref="CorvusJsonCosmosClientBuilderFactory"/> to use specific settings (as opposed
    /// to a settings provider.)
    /// </summary>
    /// <param name="settings">The settings to use.</param>
    /// <returns>The factory.</returns>
    public static CorvusJsonCosmosClientBuilderFactory FromSettings(JsonSerializerSettings settings) => new(settings);

    /// <inheritdoc />
    public CosmosClientBuilder CreateCosmosClientBuilder(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        return new CosmosClientBuilder(connectionString).WithCustomSerializer(this.GetSerializer());
    }

    /// <inheritdoc />
    public CosmosClientBuilder CreateCosmosClientBuilder(string accountEndpoint, string accountKey)
    {
        ArgumentNullException.ThrowIfNull(accountEndpoint);
        ArgumentNullException.ThrowIfNull(accountKey);

        return new CosmosClientBuilder(accountEndpoint, accountKey).WithCustomSerializer(this.GetSerializer());
    }

    /// <inheritdoc/>
    public CosmosClientBuilder CreateCosmosClientBuilder(string accountEndpoint, TokenCredential tokenCredential)
    {
        ArgumentNullException.ThrowIfNull(accountEndpoint);
        ArgumentNullException.ThrowIfNull(tokenCredential);

        return new CosmosClientBuilder(accountEndpoint, tokenCredential).WithCustomSerializer(this.GetSerializer());
    }

    /// <inheritdoc/>
    public CosmosClientOptions CreateCosmosClientOptions()
    {
        return new CosmosClientOptions
        {
            Serializer = this.GetSerializer(),
        };
    }

    private CorvusJsonDotNetCosmosSerializer GetSerializer()
        => new(this.settings ?? this.serializerSettingsProvider!.Instance);
}