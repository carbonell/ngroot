using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NGroot
{
    public class MasterLoader<TDataIdentifier>
        : MasterLoader<TDataIdentifier, NgrootSettings<TDataIdentifier>>
    {
        public MasterLoader(ICollection<Type> loaders, ICollection<Type>? testLoaders = null)
            : base(loaders, testLoaders)
        { }
    }

    public abstract class MasterLoader<TDataIdentifier, TSettings>
        where TSettings : NgrootSettings<TDataIdentifier>, new()
    {
        // It's Important to remember the order of loaders
        private readonly ICollection<Type> Loaders;

        private readonly ICollection<Type> TestLoaders;

        private Dictionary<string, object> collaborators = new Dictionary<string, object>();

        public MasterLoader(ICollection<Type> loaders, ICollection<Type>? testLoaders = null)
        {
            Loaders = loaders;
            TestLoaders = testLoaders ?? new List<Type>();
        }


        public async virtual Task ConfigureInitialData(IServiceProvider provider, string contentRootPath)
        {
            var integrationTestsSettings = provider.GetRequiredService<IOptions<TSettings>>().Value;
            if (integrationTestsSettings.SeedTestData)
                Loaders.AddRange(TestLoaders);

            foreach (var type in Loaders)
            {
                var loader = provider.GetRequiredService(type) as IModelLoader;
                if (loader != null)
                {
                    var result = await loader.LoadInitialData(contentRootPath, collaborators);
                    collaborators.TryAdd(loader.Key, result.Payloads);
                }
            }
        }
    }
}