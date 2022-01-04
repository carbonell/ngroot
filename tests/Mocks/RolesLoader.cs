using System.Threading.Tasks;
using Microsoft.Extensions.Options;
namespace NGroot.Tests
{
    public interface IRolesLoader : IModelLoader { }

    public class RolesLoader : ModelLoader<Role>, IRolesLoader
    {
        private readonly IRoleRepository _roleRepository;

        public RolesLoader(IFileLoader fileLoader, IOptions<NgrootSettings> settings,
            IRoleRepository roleRepository) : base(fileLoader, settings) => _roleRepository = roleRepository;

        public override string Key { get { return "Roles"; } }
        protected override Task<Role?> GetExistingModelAsync(Role role)
            => _roleRepository.GetByNameAsync(role.Name);

        protected override Task<Role?> CreateModelAsync(Role role)
            => _roleRepository.CreateAsync(role);

        protected override string GetFilePathRelativeToInitialData()
            => _settings.GetLoaderFilePath("Roles");
    }
}