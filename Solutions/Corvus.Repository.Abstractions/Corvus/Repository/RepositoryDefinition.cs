// <copyright file="RepositoryDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Repository
{
    /// <summary>
    /// A definition of a reposiroty.
    /// </summary>
    public class RepositoryDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryDefinition"/> class.
        /// </summary>
        public RepositoryDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryDefinition"/> class.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <param name="container">The container name.</param>
        public RepositoryDefinition(string database, string container)
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