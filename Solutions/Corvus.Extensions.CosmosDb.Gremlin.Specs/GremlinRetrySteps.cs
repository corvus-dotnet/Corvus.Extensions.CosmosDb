// <copyright file="GremlinRetrySteps.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Elements should be documented
#pragma warning disable IDE0009 // Spurious this or me qualification
#pragma warning disable RCS1192 // Spurious avoid string literals

namespace Endjin.GraphRepository.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Corvus.Extensions.CosmosDb;
    using Corvus.Retry;
    using Corvus.Retry.Policies;
    using Gremlin.Net.Driver.Exceptions;
    using Gremlin.Net.Driver.Messages;
    using Microsoft.Azure.Documents.Client;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class GremlinRetrySteps
    {
        private readonly ScenarioContext scenarioContext;

        public GremlinRetrySteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [Given(@"I execute a gremlin query")]
        public void GivenIExecuteAGremlinQuery()
        {
        }

        [Given(@"I have configured my retry options with MaxRetryAttemptsOnThrottledRequests of (.*) and MaxRetryWaitTimeInSeconds of (.*)")]
        public void GivenIHaveConfiguredARetryPolicyWithMaxRetryAttemptsOnThrottledRequestsOfAndMaxRetryWaitTimeInSecondsOf(int maxRetryAttemptsOnThrottledRequests, int maxRetryWaitTimeInSeconds)
        {
            RetryOptions retryOptions = this.scenarioContext.Get<RetryOptions>();
            retryOptions.MaxRetryAttemptsOnThrottledRequests = maxRetryAttemptsOnThrottledRequests;
            retryOptions.MaxRetryWaitTimeInSeconds = maxRetryWaitTimeInSeconds;
        }

        [When(@"The query is successful")]
        public void WhenTheQueryIsSuccessful()
        {
            GremlinClientRetryStrategy strategy = this.scenarioContext.Get<GremlinClientRetryStrategy>();
            RetryResult result = this.scenarioContext.Get<RetryResult>();

            Retriable.Retry(
                () => result.AttemptCount++,
                CancellationToken.None,
                strategy,
                new AnyException());
        }

        [When(@"The query throws an exception with a status code of (.*) and a retry-after header of (.*)")]
        public void WhenTheQueryThrowsAnExceptionWithARetry_AfterHeaderSetTo(int statusCode, string retryAfter)
        {
            GremlinClientRetryStrategy strategy = this.scenarioContext.Get<GremlinClientRetryStrategy>();

            try
            {
                Retriable.Retry(
                    this.SimulateQueryWithException(CreateGremlinClientException(statusCode, retryAfter)),
                    CancellationToken.None,
                    strategy,
                    new AnyException());
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e);
            }
        }

        [When(@"The query throws (.*) consecutive exceptions with a status code of (.*) and a retry-after header of (.*)")]
        public void WhenTheQueryThrowsConsecutiveExceptionsWithAStatusCodeOfAndARetry_AfterHeaderOf(int count, int statusCode, string retryAfter)
        {
            GremlinClientRetryStrategy strategy = this.scenarioContext.Get<GremlinClientRetryStrategy>();

            try
            {
                Retriable.Retry(
                    this.SimulateQueryWithException(CreateGremlinClientException(statusCode, retryAfter), count),
                    CancellationToken.None,
                    strategy,
                    new AnyException());
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e);
            }
        }

        [When(@"A transient exception is thrown (.*) times")]
        public void WhenATransientExceptionIsThrown(int count)
        {
            GremlinClientRetryStrategy strategy = this.scenarioContext.Get<GremlinClientRetryStrategy>();

            try
            {
                Retriable.Retry(
                    this.SimulateQueryWithException(new Exception("Transient exception"), count),
                    CancellationToken.None,
                    strategy,
                    new AnyException());
            }
            catch (Exception e)
            {
                this.scenarioContext.Set(e);
            }
        }

        [Then(@"The operation will not be retried")]
        public void ThenTheOperationWillNotBeRetried()
        {
            RetryResult result = this.scenarioContext.Get<RetryResult>();
            Assert.AreEqual(1, result.AttemptCount);
        }

        [Then(@"The operation will be retried after (.*) millisecond\(s\)")]
        public void ThenTheOperationWillBeRetriedAfterSecondS(int retriedAfter)
        {
            RetryResult result = this.scenarioContext.Get<RetryResult>();
            TimeSpan timeSinceFirstAttempt = result.TimeOfLastAttempt - result.TimeOfFirstAttempt;
            Assert.IsTrue(timeSinceFirstAttempt.TotalMilliseconds >= retriedAfter);
        }

        [Then(@"A GremlinClientException will be thrown")]
        public void ThenAGremlinClientExceptionWillBeThrown()
        {
            Exception e = this.scenarioContext.Get<Exception>();
            Assert.IsTrue(e is GremlinClientException);
        }

        [Then(@"the transient exception will be thrown")]
        public void ThenTheTransientExceptionWillBeThrown()
        {
            Exception e = this.scenarioContext.Get<Exception>();
            Assert.IsTrue(e is Exception);
            Assert.AreEqual("Transient exception", e.Message);
        }

        [Then(@"the operation will be retried (.*) times")]
        public void ThenTheOperationWillBeRetriedTimes(int retryAttempts)
        {
            RetryResult result = this.scenarioContext.Get<RetryResult>();
            Assert.AreEqual(retryAttempts + 1, result.AttemptCount);
        }

        [BeforeScenario]
        public void InitializeTest()
        {
            var retryOptions = new RetryOptions();
            this.scenarioContext.Set(retryOptions);
            var strategy = new GremlinClientRetryStrategy(retryOptions);
            this.scenarioContext.Set(strategy);
            var result = new RetryResult();
            this.scenarioContext.Set(result);
        }

        [AfterScenario]
        public void CleanUp()
        {
            _ = this.scenarioContext.Get<GremlinClientRetryStrategy>();
        }

        private static GremlinClientException CreateGremlinClientException(int statusCode, string retryAfter = null)
        {
            var attributes = new Dictionary<string, object>();
            if (retryAfter != null)
            {
                attributes.Add("x-ms-retry-after-ms", retryAfter);
            }

            attributes.Add("x-ms-status-code", statusCode);

            return new GremlinClientException("test", new ResponseException((ResponseStatusCode)statusCode, attributes, "test"));
        }

        private Action SimulateQueryWithException(Exception exceptionToThrow = null, int repeat = 1)
        {
            return () =>
            {
                RetryResult result = this.scenarioContext.Get<RetryResult>();
                if (result.TimeOfFirstAttempt == DateTime.MinValue)
                {
                    result.TimeOfFirstAttempt = DateTime.Now;
                }
                else
                {
                    result.TimeOfLastAttempt = DateTime.Now;
                }

                result.AttemptCount++;
                if (result.AttemptCount <= repeat)
                {
                    throw exceptionToThrow;
                }
            };
        }

        private class RetryResult
        {
            public int AttemptCount { get; set; }

            public DateTime TimeOfFirstAttempt { get; set; }

            public DateTime TimeOfLastAttempt { get; set; }
        }
    }
}
