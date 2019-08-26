// <copyright file="GremlinClientException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.CosmosDb
{
    using System;
    using System.Net;
    using Gremlin.Net.Driver.Exceptions;

    /// <summary>
    /// Indicates that there has been a problem executing an operation in a <see cref="ICosmosDbGremlinClient"/>.
    /// </summary>
    public class GremlinClientException : Exception
    {
        private const int RetryAfterStatusCode = 429;

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The response exception that casued the underlying problem.</param>
        public GremlinClientException(string message, ResponseException inner)
            : base(message, inner)
        {
            this.ParseInner(inner);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.</summary>
        public GremlinClientException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public GremlinClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The status code associated with the exception.</param>
        public GremlinClientException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public GremlinClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinClientException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization context.</param>
        protected GremlinClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the status code of the underlying response.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the activity ID associated with the request.
        /// </summary>
        public string AcvitityId { get; private set; }

        /// <summary>
        /// Gets the exception type associated with this exception.
        /// </summary>
        public string ExceptionType { get; private set; }

        /// <summary>
        /// Gets the timespan to retry after in the event that a 429 status code is returned.
        /// </summary>
        public TimeSpan? RetryAfter { get; private set; }

        private void ParseInner(ResponseException inner)
        {
            if (!inner.StatusAttributes.TryGetValue("x-ms-status-code", out object code))
            {
                throw new InvalidOperationException("The expected CosmosDB response header 'x-ms-status-code' was not found.");
            }

            this.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), code.ToString());

            if ((int)this.StatusCode == RetryAfterStatusCode)
            {
                if (!inner.StatusAttributes.TryGetValue("x-ms-retry-after-ms", out object time))
                {
                    throw new InvalidOperationException("The expected CosmosDB response header 'x-ms-retry-after-ms' was not found.");
                }

                this.RetryAfter = TimeSpan.Parse(time.ToString());
            }
        }
    }
}
