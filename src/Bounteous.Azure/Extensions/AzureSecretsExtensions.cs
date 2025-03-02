using Azure.Security.KeyVault.Secrets;
using Bounteous.Core.Validations;

namespace Bounteous.Azure.Extensions
{
    public static class AzureSecretsExtensions
    {
        public static KeyVaultSecret AsSecret(this string name, string value)
        {
            Validate.Begin()
                .IsNotEmpty(name, nameof(name))
                .IsNotEmpty(value, nameof(value))
                .Check();
            return new KeyVaultSecret(name, value);
        }
    }
}