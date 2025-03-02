using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Bounteous.Azure.Secrets;
using Bounteous.Core.Extensions;
using Bounteous.Core.Validations;
using FluentAssertions;
using Microsoft.AspNetCore.Routing.Constraints;
using Moq;
using Xunit;

namespace Bounteous.Azure.Test.Secrets
{
    public class KeyVaultTests
    {
        private const string KeyVaultName = "myVault";
        private readonly Mock<SecretClient> mockClient;
        private readonly KeyVault keyVault;

        public KeyVaultTests()
        {
            mockClient = new Mock<SecretClient>(new Uri($"https://{KeyVaultName}.vault.azure.net"), new DefaultAzureCredential());
            keyVault = new KeyVault(x => mockClient.Object);
        }

        [Fact]
        public void WithVaultName_ValidVaultName_ReturnsSecretsInstance()
        {
            // Act
            var result = keyVault.WithVaultName(KeyVaultName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<KeyVault>();
        }

        [Fact]
        public async Task GetKeyAsync_ValidKey_ReturnsKeyValue()
        {
            // Arrange
            var keyName = "apiKey";
            var expectedValue = "abc-123";
            
            mockClient.Setup(client => client.GetSecretAsync(keyName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(keyName, expectedValue), null!));

            // Act
            keyVault.WithVaultName(KeyVaultName);
            var actualValue = await keyVault.GetKeyAsync(keyName);

            // Assert
            actualValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task KeyNameNull()
            => await RunInvalidKeyName(null);

        [Fact]
        public async Task KeyNameEmpty()
            => await RunInvalidKeyName(string.Empty);
        
        private async Task RunInvalidKeyName(string keyName)
        {
            keyVault.WithVaultName(KeyVaultName);
            await FluentActions.Awaiting(() => keyVault.GetKeyAsync(keyName))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task GetKeyAsync_WithoutVaultName_ThrowsException()
            => await FluentActions.Awaiting(() => keyVault.GetKeyAsync("apiKey"))
                .Should().ThrowAsync<ValidationException>();

        [Fact]
        public async Task GetKeyAsync_Generic_ValidKey_ReturnsDeserializedValue()
        {
            // Arrange
            var keyName = "apiKey";
            var secret = new Secret { Uri = "www.example.com", ApiKey = "abc-123" };
            keyVault.WithVaultName(KeyVaultName);

            mockClient.Setup(client => client.GetSecretAsync(keyName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(keyName, secret.ToJson()), null!));

            // Act
            var actualValue = await keyVault.GetKeyAsync<Secret>(keyName);

            // Assert
            Validate.Begin()
                .IsNotNull(actualValue, nameof(actualValue)).Check()
                .IsEqual(actualValue.Uri, secret.Uri, nameof(actualValue.Uri))
                .IsEqual(actualValue.ApiKey, secret.ApiKey, nameof(actualValue.ApiKey))
                .Check();
        }
    }
}

public class Secret
{
    public string Uri { get; set; }
    public string ApiKey { get; set; }
}