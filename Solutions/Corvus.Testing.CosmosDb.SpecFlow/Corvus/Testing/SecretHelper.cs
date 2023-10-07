// <copyright file="SecretHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Testing
{
    using System;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Security.KeyVault.Secrets;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    ///     Helper methods for obtaining secrets.
    /// </summary>
    /// <remarks>
    /// We need this as different tenants may be configured to use different KeyVaults (in a BYO scenario).
    /// Therefore we cannot simply configure the KeyVault fallback for configuration.
    /// </remarks>
    internal static class SecretHelper
    {
        /// <summary>
        ///     Attempts to retrieve a value from configuration. If it doesn't exist, attempts to retrieve it from KeyVault.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="configurationKey">
        ///     The configuration key.
        /// </param>
        /// <param name="keyVaultName">
        ///     The key vault name.
        /// </param>
        /// <param name="keyVaultSecretName">
        ///     The key vault secret name.
        /// </param>
        /// <returns>
        ///     The secret.
        /// </returns>
        internal static async Task<string> GetSecretFromConfigurationOrKeyVaultAsync(
            IConfiguration configuration,
            string? configurationKey,
            string? keyVaultName,
            string? keyVaultSecretName)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(configurationKey);

            string? secret = configuration[configurationKey];

            if (string.IsNullOrEmpty(secret))
            {
                if (keyVaultName is not string kvn)
                {
                    throw new InvalidOperationException("Either the configuration must contain the secret, or the key vault name and the key vault secret name must be specified.");
                }

                if (keyVaultSecretName is not string kvsn)
                {
                    throw new InvalidOperationException("Either the configuration must contain the secret, or the key vault name and the key vault secret name must be specified.");
                }

                secret = await GetSecretFromKeyVaultAsync(configuration, kvn, kvsn).ConfigureAwait(false);
            }

            return secret;
        }

        /// <summary>
        ///     Retrieves a secret from a KeyVault.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="keyVaultName">
        ///     The key vault name.
        /// </param>
        /// <param name="keyVaultSecretName">
        ///     The key vault secret name.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        internal static async Task<string> GetSecretFromKeyVaultAsync(
            IConfiguration configuration,
            string keyVaultName,
            string keyVaultSecretName)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(keyVaultName);
            ArgumentException.ThrowIfNullOrEmpty(keyVaultSecretName);

            string? azureServicesAuthConnectionString = configuration["AzureServicesAuthConnectionString"];

            if (string.IsNullOrEmpty(azureServicesAuthConnectionString))
            {
                throw new InvalidOperationException("The 'AzureServicesAuthConnectionString' must be provided in the configuration to use KeyVault secrets");
            }

            var keyVaultCredentials = Corvus.Identity.ClientAuthentication.Azure.LegacyAzureServiceTokenProviderConnectionString.ToTokenCredential(azureServicesAuthConnectionString);

            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

            Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(keyVaultSecretName).ConfigureAwait(false);

            return accountKeyResponse.Value.Value;
        }
    }
}