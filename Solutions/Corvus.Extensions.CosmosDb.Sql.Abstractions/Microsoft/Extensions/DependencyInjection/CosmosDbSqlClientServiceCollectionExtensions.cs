// <copyright file="CosmosDbSqlClientServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Extensions.CosmosDb.Internal;
    using Corvus.Extensions.Json;
    using Newtonsoft.Json;

    /// <summary>
    /// Installs standard <see cref="JsonConverter"/>s.
    /// </summary>
    public static class CosmosDbSqlClientServiceCollectionExtensions
    {
        /// <summary>
        /// Add the standard sql client json converters to be picked up by the <see cref="IJsonSerializerSettingsProvider"/>.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCosmosDbSqlClientJsonConverters(this IServiceCollection services)
        {
            services.AddSingleton<JsonConverter, ConnectionPolicyConverter>();
            return services;
        }
    }
}