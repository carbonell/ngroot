using System.Threading.Tasks;

namespace NGroot.Tests;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public User(string username)
    {
        UserName = username;
    }

}

public interface IUserRepository
{
    Task<User?> CreateAsync(User r);
    Task<User?> FindByUserNameAsync(string userName);
}