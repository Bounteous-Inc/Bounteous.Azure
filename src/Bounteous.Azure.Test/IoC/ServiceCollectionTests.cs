using Bounteous.Azure.IoC;
using Bounteous.Azure.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bounteous.Azure.Test.IoC
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyVault_ShouldAddIKeyVaultService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddKeyVault();
            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IKeyVault>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<KeyVault>(service);
        }

        [Fact]
        public void AddKeyVault_ShouldAddScopedService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddKeyVault();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            using var scope1 = serviceProvider.CreateScope();
            var service1 = scope1.ServiceProvider.GetService<IKeyVault>();
            
            using var scope2 = serviceProvider.CreateScope();
            var service2 = scope2.ServiceProvider.GetService<IKeyVault>();

            // Assert
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotSame(service1, service2); // Scoped services should be different instances in different scopes
        }
    }
}