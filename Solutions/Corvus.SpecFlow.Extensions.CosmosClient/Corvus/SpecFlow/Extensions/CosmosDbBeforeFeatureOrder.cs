// <copyright file="CosmosDbBeforeFeatureOrder.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SpecFlow.Extensions
{
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines constants to use for <see cref="HookAttribute.Order"/> property on a
    /// <see cref="BeforeFeatureAttribute"/> to ensure that initialization occurs in the
    /// correct order when using <see cref="CosmosDbSetup"/> or related types.
    /// </summary>
    public static class CosmosDbBeforeFeatureOrder
    {
        /// <summary>
        /// The <c>Order</c> at which the client settings are read from configuration, and the
        /// account keys read from key vault, are made available through the <see cref="FeatureContext"/>,
        /// in tests that choose to do this.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In most tests, <see cref="CreateContainer"/> is the first step that will be used. Tests
        /// that work directly with the client objects don't really need to see the settings
        /// directly. However, some tests need the Cosmos DB account URI and key because they
        /// create new collections as part of the test. (E.g., tests that verify that we handle
        /// per-DB and per-collection throughput settings correctly in first-use scenarios need
        /// to do this.) But most tests don't care, and in some cases, this would add extra
        /// unnecessary complication to the setup. (In all the tenancy-based tests, there's no need
        /// for test to extract settings from config, because the code under test already does
        /// that job.)
        /// </para>
        /// <para>
        /// This leaves a space of 9999 values after <see cref="ContainerBeforeFeatureOrder.ServiceProviderAvailable"/>
        /// (so this is set to 20000) to make it possible to specify initialization work that can only
        /// occur after the <c>IServiceProvider</c> has been built, but before we perform Cosmos DB setup.
        /// </para>
        /// </remarks>
        public const int ReadContainerSettings = ContainerBeforeFeatureOrder.ServiceProviderAvailable + 9999;

        /// <summary>
        /// The <c>Order</c> at which the client object is created.
        /// </summary>
        public const int CreateContainer = ReadContainerSettings + 1;

        /// <summary>
        /// The <c>Order</c> from which the client object is available but has not yet been
        /// prepopulated with test data.
        /// </summary>
        /// <remarks>
        /// This leaves a space of 99 after <see cref="CreateContainer"/> (so this is 20100) to
        /// leave space for any test-specific setup that needs to happen after the client
        /// object exists, but before main test data is loaded.
        /// </remarks>
        public const int UnpopulatedDatabaseContainerAvailable = CreateContainer + 99;

        /// <summary>
        /// The <c>Order</c> at which the container object is populated.
        /// </summary>
        public const int PopulateDatabaseContainer = UnpopulatedDatabaseContainerAvailable + 1;

        /// <summary>
        /// The <c>Order</c> from which the client object is available but has not yet been
        /// prepopulated with test data.
        /// </summary>
        /// <remarks>
        /// This leaves a space of 99 after <see cref="PopulateDatabaseContainer"/> (so this is 20200) to
        /// leave space for any test-specific setup that needs to happen after the client has
        /// been prepopulated with the main test data, but before other initialization work.
        /// </remarks>
        public const int DatabaseContainerAvailable = PopulateDatabaseContainer + 99;
    }
}
