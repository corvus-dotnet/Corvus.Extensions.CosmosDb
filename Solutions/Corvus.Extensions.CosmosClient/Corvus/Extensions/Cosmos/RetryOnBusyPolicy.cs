// <copyright file="RetryOnBusyPolicy.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos
{
    using System;
    using System.Net;
    using Corvus.Retry.Policies;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A retry policy which fails immediately on all exceptions, except a Cosmos <see cref="HttpStatusCode.ServiceUnavailable"/> status code.
    /// </summary>
    /// <remarks>This allows us to use the internal retry mechanism in the SDK, but fall back on our retry when we get service unavailable errors.</remarks>
    public class RetryOnBusyPolicy : IRetryPolicy
    {
        /// <summary>
        /// Gets the shared instance of the <see cref="RetryOnBusyPolicy"/>.
        /// </summary>
        public static RetryOnBusyPolicy Instance { get; } = new RetryOnBusyPolicy();

        /// <inheritdoc/>
        public bool CanRetry(Exception exception) => exception is CosmosException dcx && dcx.StatusCode == HttpStatusCode.ServiceUnavailable;
    }
}