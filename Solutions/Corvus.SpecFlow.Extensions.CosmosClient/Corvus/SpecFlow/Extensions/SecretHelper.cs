// <copyright file="SecretHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Specflow.Extensions
{
    using System.Threading.Tasks;

    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    ///     Helper methods for obtaining secrets.
    /// </summary>
    /// <remarks>
    /// We need this as different tenants may be configured to use different keyvaults (in a BYO scenario).
    /// Therefore we cannot simply configure the keyvault fallback for configuration.
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
            string configurationKey,
            string keyVaultName,
            string keyVaultSecretName)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(configurationKey))
            {
                throw new System.ArgumentException("message", nameof(configurationKey));
            }

            string secret = configuration[configurationKey];

            if (string.IsNullOrEmpty(secret))
            {
                secret = await GetSecretFromKeyVaultAsync(configuration, keyVaultName, keyVaultSecretName).ConfigureAwait(false);
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
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(keyVaultName))
            {
                throw new System.ArgumentException("message", nameof(keyVaultName));
            }

            if (string.IsNullOrEmpty(keyVaultSecretName))
            {
                throw new System.ArgumentException("message", nameof(keyVaultSecretName));
            }

            var azureServiceTokenProvider = new AzureServiceTokenProvider(configuration["AzureServicesAuthConnectionString"]);

            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            Microsoft.Azure.KeyVault.Models.SecretBundle accountKey = await keyVaultClient
                                 .GetSecretAsync($"https://{keyVaultName}.vault.azure.net/secrets/{keyVaultSecretName}")
                                 .ConfigureAwait(false);

            return accountKey.Value;
        }
    }
}