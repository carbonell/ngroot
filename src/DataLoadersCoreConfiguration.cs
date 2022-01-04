
using Microsoft.Extensions.DependencyInjection;

namespace NGroot
{
    public static class DataLoadersCoreConfiguration
    {
        public static void InjectDependencies(IServiceCollection services)
        {
            services.AddScoped<IFileLoader, FileLoader>();
        }
    }
}