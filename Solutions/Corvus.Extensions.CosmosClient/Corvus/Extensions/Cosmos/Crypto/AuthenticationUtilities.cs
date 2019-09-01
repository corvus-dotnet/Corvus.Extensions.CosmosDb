// <copyright file="AuthenticationUtilities.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Cosmos.Crypto
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Utility functions for authentication.
    /// </summary>
    public static class AuthenticationUtilities
    {
        /// <summary>
        /// Build an authentication token.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="resourceMethod">The resource method.</param>
        /// <param name="requestDate">The request date.</param>
        /// <param name="apiKey">The API Key.</param>
        /// <returns>An auth token string for the specified parameters.</returns>
        public static string BuildAuthToken(string tenantId, string resourcePath, string resourceMethod, string requestDate, string apiKey)
        {
            using (var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(apiKey) })
            {
                string payLoad = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}\n{1}\n{2}\n{3}",
                    tenantId.ToLowerInvariant(),
                    resourcePath.ToLowerInvariant(),
                    resourceMethod.ToLowerInvariant(),
                    requestDate.ToLowerInvariant());
                byte[] hashPayLoad = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
                return Convert.ToBase64String(hashPayLoad);
            }
        }
    }
}
