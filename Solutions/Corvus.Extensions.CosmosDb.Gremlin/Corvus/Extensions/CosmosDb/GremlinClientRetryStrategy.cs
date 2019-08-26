// <copyright file="GremlinClientRetryStrategy.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using Corvus.Retry.Strategies;
    using Microsoft.Azure.Documents.Client;

    /// <summary>
    /// A retry policy for errors encountered executing graph queries.
    /// </summary>
    /// <remarks>
    /// During periods of high load CosmosDB may throttle requests, when this happens the response will include a header
    /// that indicates the period of time to wait until retrying the request. This strategy honors the retry interval received.
    /// </remarks>
    public class GremlinClientRetryStrategy : RetryStrategy
    {
        private readonly RetryOptions retryOptions;
        private IRetryStrategy transientExceptionFallbackStrategy;
        private int retryCount;
        private bool canRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientRetryStrategy"/> class.
        /// </summary>
        /// <param name="retryOptions">The retry options to apply.</param>
        public GremlinClientRetryStrategy(RetryOptions retryOptions)
        {
            this.retryOptions = retryOptions;
        }

        /// <inheritdoc/>
        public override bool CanRetry => this.canRetry;

        /// <inheritdoc/>
        public override TimeSpan PrepareToRetry(Exception lastException)
        {
            if (!(lastException is GremlinClientException graphRepositoryException) || graphRepositoryException.RetryAfter == null)
            {
                this.InitializeTransientExceptionFallbackStrategy();
                this.canRetry = this.transientExceptionFallbackStrategy.CanRetry;
                return this.transientExceptionFallbackStrategy.PrepareToRetry(lastException);
            }

            this.canRetry = this.retryCount < this.retryOptions.MaxRetryAttemptsOnThrottledRequests
               && graphRepositoryException.RetryAfter.Value <= TimeSpan.FromSeconds(this.retryOptions.MaxRetryWaitTimeInSeconds);

            this.retryCount++;

            return graphRepositoryException.RetryAfter.Value;
        }

        private void InitializeTransientExceptionFallbackStrategy()
        {
            if (this.transientExceptionFallbackStrategy == null)
            {
                this.transientExceptionFallbackStrategy = new Backoff(5, TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
