// <copyright file="CosmosDbSqlClientContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CosmosClient.Extensions.Specs.ComsosClientExtensionsFeature.Bindings
{
    using System.Linq;
    using Corvus.Testing.CosmosDb.Extensions;
    using Corvus.Testing.SpecFlow;

    using TechTalk.SpecFlow;

    /// <summary>
    /// Provides Specflow bindings for Endjin Composition.
    /// </summary>
    [Binding]
    public static class CosmosDbSqlClientContainerBindings
    {
        /// <summary>
        /// Setup the endjin container for a feature.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        /// <remarks>We expect features run in parallel to be executing in separate app domains.</remarks>
        [BeforeFeature("@perFeatureContainer", Order = ContainerBeforeFeatureOrder.PopulateServiceCollection)]
        public static void SetupFeature(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Contains("withHierarchicalPK"))
            {
                ContainerBindings.ConfigureServices(
                    featureContext,
                    serviceCollection => serviceCollection.AddSharedThroughputCosmosDbTestServices("/tenant;/id"));
            }
            else
            {
                ContainerBindings.ConfigureServices(
                    featureContext,
                    serviceCollection => serviceCollection.AddSharedThroughputCosmosDbTestServices("/id"));
            }
        }
    }
}