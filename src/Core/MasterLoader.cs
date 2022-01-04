using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NGroot
{
    public class MasterLoader<TDataIdentifier>
        : MasterLoader<TDataIdentifier, NgrootSettings<TDataIdentifier>>
        where TDataIdentifier : Enum
    {
        public MasterLoader(List<Type> loaders, List<Type>? testLoaders = null)
            : base(loaders, testLoaders)
        { }
    }

    public abstract class MasterLoader<TDataIdentifier, TSettings>
        where TDataIdentifier : Enum
        where TSettings : NgrootSettings<TDataIdentifier>, new()
    {
        // Is Important to remember the order of loaders
        private readonly List<Type> Loaders;

        private readonly List<Type> TestLoaders;

        private Dictionary<string, object> collaborators = new Dictionary<string, object>();

        public MasterLoader(List<Type> loaders, List<Type>? testLoaders = null)
        {
            Loaders = loaders ?? new List<Type>();
            TestLoaders = testLoaders ?? new List<Type>();
        }

        public async Task ConfigureInitialData(IServiceProvider provider, string contentRootPath)
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