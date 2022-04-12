using System.Collections.Generic;
using Xunit;
using System.Linq;
using Moq;
using NGroot;
using Microsoft.Extensions.Options;
using System;

namespace NGroot.Tests
{
    public class ModelLoaderTests
    {
        [Fact]
        public async void Can_Load_Role()
        {
            // Arrange
            var roleRepositoryMock = GetRoleRepositoryMock();

            var roleLoader = GetRolesLoader(roleRepository: roleRepositoryMock.Object);

            (var contentRootPath, var collaborators) = GetInitialDataParameters();
            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            Assert.True(opResult.AnySucceeded);
            roleRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Once);
        }

        [Fact]
        public async void Cant_Load_Role_WithoutLoaderHandler()
        {
            // Arrange
            var roleRepositoryMock = GetRoleRepositoryMock();
            var settings = Options.Create(new NgrootSettings { InitialDataFolderRelativePath = "C:/InitialData/", StopOnException = false });

            var roleLoader = GetRolesLoaderWithNullFileLoader(roleRepository: roleRepositoryMock.Object, settings: settings);

            (var contentRootPath, var collaborators) = GetInitialDataParameters();
            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            AssertError(opResult, "No loading source, neither file nor memory, has been setup for this loader.");
        }

        private (string, Dictionary<string, object>) GetInitialDataParameters()
        {
            return ("C:/My/Path", new Dictionary<string, object>());
        }

        private static Mock<IRoleRepository> GetRoleRepositoryMock()
        {
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(r => r.CreateAsync(It.IsAny<Role>())).ReturnsAsync(new Role("Admin"));
            return roleRepository;
        }

        private RolesLoader GetRolesLoader(IFileLoader? fileLoader = null, IOptions<NgrootSettings>? settings = null,
        IRoleRepository? roleRepository = null)
        {

            var roles = new List<Role> { new Role("Admin") };

            var fileLoaderMock = new Mock<IFileLoader>();
            fileLoaderMock.Setup(r => r.LoadFile<Role>(It.IsAny<string>())).ReturnsAsync(roles);
            fileLoader = fileLoader ?? fileLoaderMock.Object;

            var roleRepositoryMock = GetRoleRepositoryMock();
            roleRepository = roleRepository ?? roleRepositoryMock.Object;

            var grootSettings = new NgrootSettings
            {
                InitialDataFolderRelativePath = "C:/InitialData/",
                PathConfiguration = new List<BaseDataSettings<string>> { new BaseDataSettings<string> { Identifier = "Roles", RelativePath = "Roles/Roles.json" } }
            };
            settings = settings ?? Options.Create(grootSettings);

            return new RolesLoader(fileLoader, settings, roleRepository);
        }

        private RolesLoader GetRolesLoaderWithNullFileLoader(IOptions<NgrootSettings>? settings = null,
        IRoleRepository? roleRepository = null)
        {

            var roles = new List<Role> { new Role("Admin") };


            var roleRepositoryMock = GetRoleRepositoryMock();
            roleRepository = roleRepository ?? roleRepositoryMock.Object;

            var grootSettings = new NgrootSettings
            {
                InitialDataFolderRelativePath = "C:/InitialData/",
                PathConfiguration = new List<BaseDataSettings<string>> { new BaseDataSettings<string> { Identifier = "Roles", RelativePath = "Roles/Roles.json" } }
            };
            settings = settings ?? Options.Create(grootSettings);

            return new RolesLoader(null!, settings, roleRepository);
        }

        [Fact]
        public async void Can_Load_Existing_Role()
        {
            // Arrange
            var roleRepositoryMock = GetRoleRepositoryMock();
            var existingRole = new Role("Admin") { Id = 1 };
            roleRepositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync(existingRole);

            var roleLoader = GetRolesLoader(roleRepository: roleRepositoryMock.Object);
            (var contentRootPath, var collaborators) = GetInitialDataParameters();


            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            Assert.False(opResult.AnySucceeded);
            var errorMessage = opResult.Errors.FirstOrDefault();
            Assert.Contains("This model was already added.", errorMessage);

            var payload = opResult.Payloads.FirstOrDefault();
            Assert.NotNull(payload);
            if (payload != null)
                Assert.Equal(1, payload.Id);
        }
        private static void AssertError<TModel>(BatchOperationResult<TModel> opResult, string expectedError)
        where TModel : class
        {
            Assert.False(opResult.AnySucceeded);
            var errorMessage = String.Join(",", opResult.Errors);
            Assert.Equal(expectedError, errorMessage);
        }

        [Fact]
        public async void Cant_Load_Data_With_Null_Initial_Data_Path_WhenLoadingFromFile()
        {
            // Arrange
            var settings = Options.Create(new NgrootSettings { StopOnException = false });
            var roleLoader = GetRolesLoader(settings: settings);
            var (contentRootPath, collaborators) = GetInitialDataParameters();

            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert

            AssertError(opResult, "Initial data path not set.");
        }


        [Fact]
        public async void Cant_Load_Data_With_Null_File_Path()
        {
            // Arrange
            var settings = Options.Create(new NgrootSettings { InitialDataFolderRelativePath = "C:/InitialData/", StopOnException = false });
            var roleLoader = GetRolesLoader(settings: settings);
            (var contentRootPath, var collaborators) = GetInitialDataParameters();

            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            AssertError(opResult, "File path not set for RolesLoader.");
        }

        [Fact]
        public async void Can_Load_Model_With_Fluent_Api()
        {
            // Arrange
            var userRepositoryMock = GetUserRepositoryMock();
            var userLoader = GetUsersLoader(userRepository: userRepositoryMock.Object);
            (var contentRootPath, var collaborators) = GetInitialDataParameters();
            // Act
            var opResult = await userLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            Assert.True(opResult.AnySucceeded);
            userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        private UsersLoader GetUsersLoader(IFileLoader? fileLoader = null, IOptions<NgrootSettings>? settings = null,
        IUserRepository? userRepository = null)
        {

            var users = new List<User> { new User("Joe") };

            var fileLoaderMock = new Mock<IFileLoader>();
            fileLoaderMock.Setup(r => r.LoadFile<User>(It.IsAny<string>())).ReturnsAsync(users);
            fileLoader = fileLoader ?? fileLoaderMock.Object;

            var userRepositoryMock = GetUserRepositoryMock();
            userRepository = userRepository ?? userRepositoryMock.Object;

            var grootSettings = new NgrootSettings
            {
                InitialDataFolderRelativePath = "C:/InitialData/",
                PathConfiguration = new List<BaseDataSettings<string>> { new BaseDataSettings<string> { Identifier = "Users", RelativePath = "Users/Users.json" } }
            };
            settings = settings ?? Options.Create(grootSettings);

            return new UsersLoader(fileLoader, settings, userRepository);
        }

        private UsersLoaderWith2Handlers GetUsersWith2HandlersLoader(IFileLoader? fileLoader = null, IOptions<NgrootSettings>? settings = null,
        IUserRepository? userRepository = null)
        {

            var users = new List<User> { new User("Joe") };

            var fileLoaderMock = new Mock<IFileLoader>();
            fileLoaderMock.Setup(r => r.LoadFile<User>(It.IsAny<string>())).ReturnsAsync(users);
            fileLoader = fileLoader ?? fileLoaderMock.Object;

            var userRepositoryMock = GetUserRepositoryMock();
            userRepository = userRepository ?? userRepositoryMock.Object;

            var grootSettings = new NgrootSettings
            {
                InitialDataFolderRelativePath = "C:/InitialData/",
                PathConfiguration = new List<BaseDataSettings<string>> { new BaseDataSettings<string> { Identifier = "Users", RelativePath = "Users/Users.json" } }
            };
            settings = settings ?? Options.Create(grootSettings);

            return new UsersLoaderWith2Handlers(fileLoader, settings, userRepository);
        }

        private static Mock<IUserRepository> GetUserRepositoryMock()
        {
            var roleRepository = new Mock<IUserRepository>();
            roleRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(new User("Admin"));
            return roleRepository;
        }

        [Fact]
        public async void Cant_Load_Existing_Model_With_Fluent_Api()
        {
            // Arrange
            var userRepositoryMock = GetUserRepositoryMock();
            userRepositoryMock.Setup(s => s.FindByUserNameAsync(It.IsAny<string>())).ReturnsAsync(new User("Joe"));
            var userLoader = GetUsersLoader(userRepository: userRepositoryMock.Object);
            (var contentRootPath, var collaborators) = GetInitialDataParameters();
            // Act
            var opResult = await userLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            AssertError(opResult, "This model was already added.");
        }


        [Fact]
        public async void Cant_Load_Model_With_Both_FileHandlersAndInMemoryHandlers()
        {
            // Arrange
            var settings = Options.Create(new NgrootSettings { InitialDataFolderRelativePath = "C:/InitialData/", StopOnException = false });
            var userLoader = GetUsersWith2HandlersLoader(settings: settings);

            (var contentRootPath, var collaborators) = GetInitialDataParameters();
            // Act
            var opResult = await userLoader.ConfigureInitialDataAsync(contentRootPath, collaborators);

            // Assert
            AssertError(opResult, "Two loading sources have been setup for this loader. Plese specify just one.");
        }

        [Fact]
        public async void Can_LoadModelWithCollaborators()
        {
            // Arrange
            var batchResult = new BatchOperationResult<AssignedPermission>();
            var repository = new AssignedPermissionsRepository();
            var loader = GetAssignedPermissionsLoader(assignedPermissionsRepository: repository);
            var collaborators = GetAssignedPermissionsCollaborators();

            // Act
            batchResult = await loader.ConfigureInitialDataAsync("my/path", collaborators);

            // Assert
            Assert.True(batchResult.AllSucceeded);
            var admin1Permission = batchResult.Payloads.ElementAt(0);
            var admin2Permission = batchResult.Payloads.ElementAt(1);
            Assert.Equal(2, batchResult.Payloads.Count());

            Assert.Equal(1, admin1Permission?.RoleId);
            Assert.Equal(1, admin1Permission?.PermissionId);

            Assert.Equal(2, admin2Permission?.RoleId);
            Assert.Equal(2, admin2Permission?.PermissionId);
        }

        private AssignedPermissionsLoader GetAssignedPermissionsLoader(IOptions<NgrootSettings>? settings = null, IAssignedPermissionsRepository? assignedPermissionsRepository = null)
        {
            var profileRoles = AssignedPermissionsLoader.GetAssignedPermissions();
            var assignedPermissionRepositoryMock = GetAssignedPermissionsRepositoryMock();
            assignedPermissionsRepository = assignedPermissionsRepository ?? assignedPermissionRepositoryMock.Object;

            var grootSettings = new NgrootSettings
            {
                InitialDataFolderRelativePath = "C:/InitialData/",
                PathConfiguration = new List<BaseDataSettings<string>> { new BaseDataSettings<string> { Identifier = "AssignedPermissions", RelativePath = "AssignedPermissions/AssignedPermissions.json" } }
            };
            settings = settings ?? Options.Create(grootSettings);

            return new AssignedPermissionsLoader(settings, assignedPermissionsRepository);
        }

        private static Mock<IAssignedPermissionsRepository> GetAssignedPermissionsRepositoryMock()
        {
            var assignedPermissionRepository = new Mock<IAssignedPermissionsRepository>();
            assignedPermissionRepository.Setup(r => r.CreateAsync(It.IsAny<AssignedPermission>())).ReturnsAsync(new AssignedPermission());
            return assignedPermissionRepository;
        }


        private Dictionary<string, object> GetAssignedPermissionsCollaborators()
        {
            var collaborators = new Dictionary<string, object>();
            var roles = GetRoles().Select(r => (object)r);
            var profiles = GetPermissions().Select(p => (object)p);
            collaborators.Add("Roles", roles);
            collaborators.Add("Permissions", profiles);
            return collaborators;
        }

        private List<Role> GetRoles()
        {
            return new List<Role>
            {
                new Role("Admin1"){
                    Id = 1,
                },
                new Role("Admin2"){
                    Id = 2,
                }
            };
        }

        private static List<Permission> GetPermissions()
        {
            return new List<Permission>{
                new Permission("Users.Create"){
                    Id = 1
                },
                new Permission("Users.Edit"){
                    Id = 2
                }
            };
        }


    }

}