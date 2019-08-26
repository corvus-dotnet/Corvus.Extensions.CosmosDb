// <copyright file="CosmosDbContextKeys.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions.CosmosDb
{
    /// <summary>
    /// Keys used to store items in SpecFlow feature context relating to Cosmos DB tests.
    /// </summary>
    public static class CosmosDbContextKeys
    {
        /// <summary>
        /// The key for the Cosmos DB client instance in the feature context.
        /// </summary>
        public const string CosmosDbClient = "CosmosDbSqlClient";

        /// <summary>
        /// The key for the Cosmos DB AccountKey (secret) in the feature context.
        /// </summary>
        public const string AccountKey = "CosmosDbAccountKey";

        /// <summary>
        /// Key used to store exceptions in the SpecFlow scenario context.
        /// </summary>
        public const string ExceptionKey = "Exception";
    }
}
