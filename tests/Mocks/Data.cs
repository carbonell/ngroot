using System.Threading.Tasks;

namespace NGroot.Tests
{
    public class Role
    {
        public Role(string name)
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

    }

    public interface IRoleRepository
    {
        Task<Role?> CreateAsync(Role r);
        Task<Role?> GetByNameAsync(string name);
    }

    public class InitialDataSettingsSeam
    {

    }
}