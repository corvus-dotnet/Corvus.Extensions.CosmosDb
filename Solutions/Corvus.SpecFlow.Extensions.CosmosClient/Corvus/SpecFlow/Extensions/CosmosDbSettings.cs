// <copyright file="CosmosDbSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions
{
    using System;

    /// <summary>
    /// Settings for CosmosDB databases and collections used in tests.
    /// </summary>
    public class CosmosDbSettings
    {
        /// <summary>
        /// Gets or sets the URI for the Cosmos DB Account.
        /// </summary>
        public string CosmosDbAccountUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the secret to look up in Key Vault to use when authenticating
        /// to Cosmos DB.
        /// </summary>
        public string CosmosDbKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to use within the Cosmos DB account.
        /// </summary>
        public string CosmosDbDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the default throughput offer to specify.
        /// </summary>
        public int CosmosDbDefaultOfferThroughput { get; set; }
    }
}
