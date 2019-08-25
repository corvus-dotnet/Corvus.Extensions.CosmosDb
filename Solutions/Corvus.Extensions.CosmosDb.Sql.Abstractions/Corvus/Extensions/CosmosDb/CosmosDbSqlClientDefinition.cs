// <copyright file="CosmosDbSqlClientDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    /// <summary>
    /// A definition of a Sql client database.
    /// </summary>
    public class CosmosDbSqlClientDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClientDefinition"/> class.
        /// </summary>
        public CosmosDbSqlClientDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbSqlClientDefinition"/> class.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="container">The container name.</param>
        public CosmosDbSqlClientDefinition(string database, string container)
        {
            this.Database = database;
            this.Container = container;
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string Container { get; set; }
    }
}