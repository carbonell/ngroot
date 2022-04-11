using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    ) : base(settings)
    {
        Setup("Users")
        .FindDuplicatesWith(m => userRepository.FindByUserNameAsync(m.UserName))
        .CreateModelUsing(m => userRepository.CreateAsync(m))
        .UseFileLoader(fileLoader);
    }
}

public class UsersLoaderWith2Handlers
    : ModelLoader<User>,
    IUsersLoader

{
    public UsersLoaderWith2Handlers(
        IFileLoader fileLoader,
        IOptions<NgrootSettings> settings,
        IUserRepository userRepository
    ) : base(settings)
    {
        Setup("Users")
        .FindDuplicatesWith(m => userRepository.FindByUserNameAsync(m.UserName))
        .CreateModelUsing(m => userRepository.CreateAsync(m))
        .UseFileLoader(fileLoader)
        .Load(() => Task.FromResult(new List<User>()));
    }

}