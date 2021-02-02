// <copyright file="CosmosDbContextKeys.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Testing.CosmosDb.SpecFlow
{
    /// <summary>
    /// Keys used to store items in SpecFlow feature context relating to Cosmos DB tests.
    /// </summary>
    public static class CosmosDbContextKeys
    {
        /// <summary>
        /// The key for the Cosmos DB client instance in the feature context.
        /// </summary>
        public const string CosmosDbClient = "CosmosDbClient";

        /// <summary>
        /// The key for the Cosmos DB container instance in the feature context.
        /// </summary>
        public const string CosmosDbContainer = "CosmosDbContainer";

        /// <summary>
        /// The key for the Cosmos DB database instance in the feature context.
        /// </summary>
        public const string CosmosDbDatabase = "CosmosDbDatabase";

        /// <summary>
        /// The key for the Cosmos DB partition key path in the feature context.
        /// </summary>
        public const string PartitionKeyPath = "CosmosDbPartitionKeyPath";

        /// <summary>
        /// The key for the Cosmos DB AccountKey (secret) in the feature context.
        /// </summary>
        public const string AccountKey = "CosmosAccountKey";

        /// <summary>
        /// Key used to store exceptions in the SpecFlow scenario context.
        /// </summary>
        public const string ExceptionKey = "Exception";
    }
}
