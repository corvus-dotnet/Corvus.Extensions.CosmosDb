// <copyright file="SecretHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Testing
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Core;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;

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

            string azureServicesAuthConnectionString = configuration["AzureServicesAuthConnectionString"];

            // Irritatingly, v12 of the Azure SDK has done away with the AppAuthentication connection
            // strings that AzureServiceTokenProvider used to support, making it very much harder to
            // allow an application to switch between different modes of authentication via configuration.
            // This code supports some of the ones we often use.
            const string appIdPattern = "RunAs=App;AppId=(?<AppId>[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12});TenantId=(?<TenantId>[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12});AppKey=(?<AppKey>[^;]*)";
            TokenCredential keyVaultCredentials = (azureServicesAuthConnectionString?.Trim() ?? string.Empty) switch
            {
#pragma warning disable SA1122 // Use string.Empty for empty strings - StyleCop analyzer 1.1.118 doesn't understand patterns; it *has* to be "" here
                "" => new DefaultAzureCredential(),
#pragma warning restore SA1122 // Use string.Empty for empty strings

                "RunAs=Developer;DeveloperTool=AzureCli" => new AzureCliCredential(),
                "RunAs=Developer;DeveloperTool=VisualStudio" => new VisualStudioCredential(),
                "RunAs=App" => new ManagedIdentityCredential(),

                string s when Regex.Match(s, appIdPattern) is Match m && m.Success =>
                    new ClientSecretCredential(m.Groups["TenantId"].Value, m.Groups["AppId"].Value, m.Groups["AppKey"].Value),

                _ => throw new InvalidOperationException($"AzureServicesAuthConnectionString configuration value '{azureServicesAuthConnectionString}' is not supported in this version of Corvus Tenancy")
            };

            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

            Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(keyVaultSecretName).ConfigureAwait(false);

            return accountKeyResponse.Value.Value;
        }
    }
}