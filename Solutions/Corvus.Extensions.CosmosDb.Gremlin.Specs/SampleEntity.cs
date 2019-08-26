// <copyright file="SampleEntity.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Endjin.GraphRepository.Specs
{
    /// <summary>
    /// An example entity for client specs.
    /// </summary>
    public class SampleEntity
    {
        /// <summary>
        /// Gets or sets a unique ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value.
        /// </summary>
        public int SomeValue { get; set; }

        /// <summary>
        /// Gets a value used to work around the fact that CosmosDB won't let us use the ID as the partition id.
        /// </summary>
        public string Partition => this.Id;
    }
}
