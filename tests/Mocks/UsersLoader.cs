using System;
using Microsoft.Extensions.Options;

namespace NGroot.Tests;
public interface IUsersLoader
    : IModelLoader
{ }

public class UsersLoader
    : ModelLoader<User>,
    IUsersLoader
{
    public UsersLoader(
        IFileLoader fileLoader,
        IOptions<NgrootSettings> settings,
        IUserRepository userRepository
    ) : base(fileLoader, settings)
    {
        SetupLoader("Users")
        .FindDuplicatesWith(m => userRepository.FindByUserNameAsync(m.UserName))
        .CreateModelUsing(m => userRepository.CreateAsync(m));
    }
}