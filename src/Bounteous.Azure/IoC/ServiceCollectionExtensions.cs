using Bounteous.Azure.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace Bounteous.Azure.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeyVault(this IServiceCollection serviceCollection)
            => serviceCollection.AddScoped<IKeyVault, KeyVault>();
    }
}