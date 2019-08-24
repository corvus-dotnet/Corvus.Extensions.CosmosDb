// <copyright file="RepositoryContainerBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Repository.Specs
{
    using Corvus.SpecFlow.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Provides Specflow bindings for Endjin Composition.
    /// </summary>
    [Binding]
    public static class RepositoryContainerBindings
    {
        /// <summary>
        /// Setup the endjin container for a feature.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        /// <remarks>We expect features run in parallel to be executing in separate app domains.</remarks>
        [BeforeFeature("@setupContainer", Order = ContainerBeforeFeatureOrder.PopulateServiceCollection)]
        public static void SetupFeature(FeatureContext featureContext)
        {
            ContainerBindings.ConfigureServices(
                featureContext,
                serviceCollection =>
                {
                    serviceCollection.AddDefaultJsonSerializationSettings();
                    serviceCollection.AddSharedThroughputCosmosDbTestServices("/id");
                });
        }
    }
}
