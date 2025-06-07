// <copyright file="CosmosDbSqlClientContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions.Specs.CosmosClientExtensionsFeature.Bindings
{
    using Corvus.Testing.CosmosDb.Extensions;
    using Corvus.Testing.ReqnRoll;

    using Reqnroll;

    /// <summary>
    /// Provides Specflow bindings for Endjin Composition.
    /// </summary>
    [Binding]
    public static class CosmosDbSqlClientContainerBindings
    {
        /// <summary>
        /// Set up the endjin container for a feature.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        /// <remarks>We expect features run in parallel to be executing in separate app domains.</remarks>
        [BeforeFeature("@perFeatureContainer", Order = ContainerBeforeFeatureOrder.PopulateServiceCollection)]
        public static void SetupFeature(FeatureContext featureContext)
        {
            ContainerBindings.ConfigureServices(
                featureContext,
                serviceCollection => serviceCollection.AddSharedThroughputCosmosDbTestServices("/id"));
        }
    }
}