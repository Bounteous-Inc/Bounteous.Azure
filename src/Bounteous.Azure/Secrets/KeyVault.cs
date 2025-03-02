using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Bounteous.Core.Extensions;
using Bounteous.Core.Validations;

namespace Bounteous.Azure.Secrets
{
    public class KeyVault
    {
        private readonly Func<string, SecretClient> clientFactory;
        private string keyVaultUri;

        public KeyVault() { }
    
        public KeyVault(Func<string, SecretClient> clientFactory)
        {
            Validate.Begin().IsNotNull(clientFactory, nameof(clientFactory)).Check();
            this.clientFactory = clientFactory;
        }
    
        public KeyVault WithVaultName(string keyVaultName)
        {
            Validate.Begin().IsNotEmpty(keyVaultName, nameof(keyVaultName)).Check();
            keyVaultUri = $"https://{keyVaultName}.vault.azure.net";
            return this;
        }

        private SecretClient Client => clientFactory != null
            ? clientFactory(keyVaultUri)
            : new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

        public async Task<string> GetKeyAsync(string keyName)
        {
            Validate.Begin()
                .IsNotNull(keyVaultUri, "Key Vault name must be provided using WithVaultName method.")
                .IsNotEmpty(keyName, "keyName must be provided.")
                .Check();

            KeyVaultSecret secret = await Client.GetSecretAsync(keyName);
            return secret.Value;
        }

        public async Task<T> GetKeyAsync<T>(string keyName)
        {
            var secret = await GetKeyAsync(keyName);
            return secret.FromJson<T>();
        }
    }
}