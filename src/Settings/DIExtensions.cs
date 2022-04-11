
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NGroot
{
    public static class DIExtensions
    {

        public static void ConfigureNGroot<TKey>(this IServiceCollection services, Action<NgrootSettings<TKey>> settingsBuilder, Assembly? loaderAssemblySource = null)
        {
            services.Configure<NgrootSettings<TKey>>(settingsBuilder);
            var settings = new NgrootSettings<TKey>();
            settingsBuilder(settings);
            if (loaderAssemblySource != null)
            {
                services.RegisterLoaders(typeof(ModelLoader<,>), loaderAssemblySource);
            }
        }

        public static void ConfigureNGroot(this IServiceCollection services, Action<NgrootSettings> settingsBuilder, Assembly? loaderAssemblySource = null)
        {
            var settings = new NgrootSettings();
            services.Configure<NgrootSettings>(settingsBuilder);
            if (loaderAssemblySource != null)
            {
                services.RegisterLoaders(typeof(ModelLoader<>), loaderAssemblySource);
            }
        }

        public static void RegisterLoaders(this IServiceCollection services, Type type, Assembly assembly)
        {
            foreach (var loader in assembly.GetTypes()
                .Where(t => type.IsAssignableFrom(t) && !t.Equals(type)))
            {
                services.Add(new ServiceDescriptor(loader, loader, ServiceLifetime.Transient));
            }
        }

    }
}