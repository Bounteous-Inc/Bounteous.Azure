using Bounteous.Azure.Extensions;
using Bounteous.Azure.Test.Models;
using Bounteous.Core.Extensions;
using Bounteous.Core.Validations;
using FluentAssertions;
using Xunit;

namespace Bounteous.Azure.Test.Extensions
{
    public class SecretsTests
    {
        private readonly Secret secretValue = new Secret { Uri = "www.bounteous.com", ApiKey = "abc-123" };
        [Fact]
        public void AsSecret()
        {
            var secret = "secretName".AsSecret(secretValue.ToJson());

            Validate.Begin()
                .IsNotNull(secret, nameof(secret)).Check()
                .IsEqual(secret.Name, "secretName", nameof(secret.Name))
                .IsEqual(secret.Value, secretValue.ToJson(), nameof(secret.Value))
                .Check();
        }
        
        [Fact]
        public void SecretNameNull()
            =>  FluentActions.Invoking(() => ((string)null).AsSecret(secretValue.ToJson()))
                .Should().Throw<ValidationException>();
        
        [Fact]
        public void SecretNameEmpty()
            =>  FluentActions.Invoking(() => string.Empty.AsSecret(secretValue.ToJson()))
                .Should().Throw<ValidationException>();
        
        [Fact]
        public void SecretValueNull()
            =>  FluentActions.Invoking(() => "boo".AsSecret(null))
                .Should().Throw<ValidationException>();
        
        [Fact]
        public void SecretValueEmpty()
            =>  FluentActions.Invoking(() => "boo".AsSecret(string.Empty))
                .Should().Throw<ValidationException>();
    }
}