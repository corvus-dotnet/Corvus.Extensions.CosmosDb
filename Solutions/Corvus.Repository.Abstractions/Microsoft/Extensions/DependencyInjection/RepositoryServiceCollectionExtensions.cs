// <copyright file="RepositoryServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Repository.Internal;
    using Newtonsoft.Json;

    /// <summary>
    /// Installs standard <see cref="JsonConverter"/>s.
    /// </summary>
    public static class RepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// Add the standard Endjin Repository components.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRepositoryJsonConverters(this IServiceCollection services)
        {
            services.AddSingleton<JsonConverter, ConnectionPolicyConverter>();
            return services;
        }
    }
}