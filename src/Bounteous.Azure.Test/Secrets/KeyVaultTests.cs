using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Bounteous.Azure.Extensions;
using Bounteous.Azure.Secrets;
using Bounteous.Azure.Test.Models;
using Bounteous.Core.Extensions;
using Bounteous.Core.Validations;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bounteous.Azure.Test.Secrets
{
public class KeyVaultTests
    {
        private const string KeyVaultName = "myVault";
        private const string SecretName = "api-key";
        private const string SecretValue = "abc-123";
        private readonly Mock<SecretClient> mockClient;
        private readonly KeyVault keyVault;

        public KeyVaultTests()
        {
            mockClient = new Mock<SecretClient>(new Uri($"https://{KeyVaultName}.vault.azure.net"), new DefaultAzureCredential());
            keyVault = new KeyVault(x => mockClient.Object);
        }

        [Fact]
        public void WithVaultName() 
            => keyVault.WithVaultName(KeyVaultName).Should().BeSameAs(keyVault);

        [Fact]
        public async Task WithVaultName_Null()
            => await FluentActions.Awaiting(() => keyVault.GetKeyAsync(null))
                .Should().ThrowAsync<ValidationException>();
        
        [Fact]
        public async Task WithVaultName_Empty()
            => await FluentActions.Awaiting(() => keyVault.GetKeyAsync(string.Empty))
                .Should().ThrowAsync<ValidationException>();

        [Fact]
        public async Task GetKeyAsync_ValidKey_ReturnsKeyValue()
        {
            mockClient.Setup(client => client.GetSecretAsync(SecretName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(SecretName.AsSecret(SecretValue), null!));

            // Act
            keyVault.WithVaultName(KeyVaultName);
            var actualValue = await keyVault.GetKeyAsync(SecretName);

            // Assert
            actualValue.Should().Be(SecretValue);
        }

        [Fact]
        public async Task KeyNameNull() => await RunInvalidKeyName(null);

        [Fact]
        public async Task KeyNameEmpty() => await RunInvalidKeyName(string.Empty);
        
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
            var secretObject = new Secret { Uri = "www.example.com", ApiKey = "abc-123" };
            keyVault.WithVaultName(KeyVaultName);

            mockClient.Setup(client => client.GetSecretAsync(SecretName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(SecretName.AsSecret(secretObject.ToJson()), null!));

            // Act
            var actualValue = await keyVault.GetKeyAsync<Secret>(SecretName);

            // Assert
            Validate.Begin()
                .IsNotNull(actualValue, nameof(actualValue)).Check()
                .IsEqual(actualValue.Uri, secretObject.Uri, nameof(actualValue.Uri))
                .IsEqual(actualValue.ApiKey, secretObject.ApiKey, nameof(actualValue.ApiKey))
                .Check();
        }

        [Fact]
        public void WithCredentials_ValidCredential_SetsCredential()
            => keyVault.WithCredentials(new Mock<TokenCredential>().Object).Should().BeOfType<KeyVault>();

        [Fact]
        public async Task GetKeyAsync_WithProvidedCredential_UsesProvidedCredential()
        {
            var mockCredential = new Mock<TokenCredential>();
            keyVault.WithVaultName(KeyVaultName).WithCredentials(mockCredential.Object);

            mockClient.Setup(client => client.GetSecretAsync(SecretName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(SecretName.AsSecret(SecretValue), null!));

            // Act
            var actualValue = await keyVault.GetKeyAsync(SecretName);

            // Assert
            actualValue.Should().Be(SecretValue);
            mockClient.Verify(client => 
                client.GetSecretAsync(SecretName, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetKeyAsync_WithoutProvidedCredential_UsesDefaultAzureCredential()
        {
            keyVault.WithVaultName(KeyVaultName);

            mockClient.Setup(client => client.GetSecretAsync(SecretName, null, CancellationToken.None))
                .ReturnsAsync(Response.FromValue(SecretName.AsSecret(SecretValue), null!));

            // Act
            var actualValue = await keyVault.GetKeyAsync(SecretName);

            // Assert
            actualValue.Should().Be(SecretValue);
            mockClient.Verify(client => client.GetSecretAsync(SecretName, null, CancellationToken.None), Times.Once);
        }
    }
}