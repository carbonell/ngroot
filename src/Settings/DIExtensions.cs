
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
                services.RegisterLoaders(typeof(ModelLoader<,>), loaderAssemblySource);
            }
        }

        public static void RegisterLoaders(this IServiceCollection services, Type type, Assembly assembly)
        {
            var types = assembly.GetTypes()
            // ;
            .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type);
            foreach (var loader in types)
            {
                services.Add(new ServiceDescriptor(loader, loader, ServiceLifetime.Transient));
            }
        }

        public static async Task LoadData<TKey>(
            this IServiceProvider provider,
            IEnumerable<Type> loaders,
            string contentRootPath = "",
            IEnumerable<Type>? testLoaders = null
        )
        {

            testLoaders = testLoaders ?? new List<Type>();
            { };
            var masterLoader = new MasterLoader<TKey>(loaders.ToList(), testLoaders.ToList());
            await masterLoader.ConfigureInitialData(provider, contentRootPath);
        }

    }
}