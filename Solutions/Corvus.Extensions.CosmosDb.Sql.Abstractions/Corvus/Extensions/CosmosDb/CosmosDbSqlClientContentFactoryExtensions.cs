// <copyright file="CosmosDbSqlClientContentFactoryExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using Corvus.ContentHandling;
    using Corvus.Extensions.CosmosDb.Internal;

    /// <summary>
    /// Extension methods to register CosmosDbSqlClient-related content.
    /// </summary>
    public static class CosmosDbSqlClientContentFactoryExtensions
    {
        /// <summary>
        /// Registers content with the container.
        /// </summary>
        /// <param name="factory">The <see cref="ContentFactory"/>.</param>
        /// <returns>The content factory with the content registered.</returns>
        public static ContentFactory RegisterCosmosDbSqlClientContent(this ContentFactory factory)
        {
            factory.RegisterTransientContent<CosmosDbSqlClientConfiguration>();
            return factory;
        }
    }
}
