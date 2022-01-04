using System.Collections.Generic;
using Xunit;
using System.Linq;
using Moq;
using NGroot;
using Microsoft.Extensions.Options;

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

            settings = settings ?? Options.Create(new NgrootSettings());

            return new RolesLoader(fileLoader, settings, roleRepository);

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
            var payload = opResult.Payloads.FirstOrDefault();
            Assert.NotNull(payload);
            Assert.Equal(1, payload.Id);
            var errorMessage = opResult.Errors.ToRawString();
            Assert.Contains("This model was already added.", errorMessage);
        }

        [Fact]
        public async void Cant_Load_Data_With_Null_Content_Root_Path()
        {
            var fileParser = new Mock<IFileLoader>();
            var roles = new List<Role> { new Role("Customer") };
            fileParser.Setup(r => r.LoadFile<Role>(It.IsAny<string>())).ReturnsAsync(roles);
            var roleRepository = new Mock<IRoleRepository>();
            var roleResult = new Role("Admin");
            roleRepository.Setup(r => r.CreateAsync(It.IsAny<Role>())).ReturnsAsync(roleResult);
            var settings = new InitialDataSettingsSeam();
            var roleLoader = new RolesLoader(fileParser.Object, settings, roleRepository.Object);
            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync("");

            // Assert
            Assert.False(opResult.AnySucceeded);
            var message = opResult.Errors.ToRawString();
            Assert.Equal("content root path not set for RolesLoader.", message);
        }


        [Fact]
        public async void Cant_Load_Data_With_Null_Initial_Data_Path()
        {
            var fileParser = new Mock<IFileLoader>();
            var roles = new List<Role> { new Role("Admin") { Id = 1 } };
            fileParser.Setup(r => r.LoadFile<Role>(It.IsAny<string>())).ReturnsAsync(roles);
            var roleRepository = new Mock<IRoleRepository>();
            var roleResult = new Role("Customer");
            roleRepository.Setup(r => r.CreateAsync(It.IsAny<Role>())).ReturnsAsync(roleResult);
            var settings = new Mock<IOptions<InitialDataSettings>>();
            settings.Setup(s => s.Value).Returns(new InitialDataSettings());
            var roleLoader = new RolesLoader(fileParser.Object, settings.Object, roleRepository.Object);
            var contentRootPath = "C:/My/Path";

            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath);

            // Assert
            Assert.False(opResult.AnySucceeded);
            var message = opResult.Errors.ToRawString();

            Assert.Equal("Initial data path not set.", message);
        }


        [Fact]
        public async void Cant_Load_Data_With_Null_File_Path()
        {
            var fileParser = new Mock<IFileLoader>();
            var roles = new List<Role> { new Role("Admin") };
            fileParser.Setup(r => r.LoadFile<Role>(It.IsAny<string>())).ReturnsAsync(roles);
            var roleRepository = new Mock<IRoleRepository>();
            var roleResult = new Role("Admin");
            roleRepository.Setup(r => r.CreateAsync(It.IsAny<Role>())).ReturnsAsync(roleResult);
            var settings = new Mock<IOptions<InitialDataSettings>>();
            settings.Setup(s => s.Value).Returns(new InitialDataSettings { InitialDataFolderRelativePath = "C:/InitialData/" });
            var roleLoader = new RolesLoader(fileParser.Object, settings.Object, roleRepository.Object);
            var contentRootPath = "C:/My/Path";

            // Act
            var opResult = await roleLoader.ConfigureInitialDataAsync(contentRootPath);

            // Assert
            Assert.False(opResult.AnySucceeded);
            var message = opResult.Errors.ToRawString();
            Assert.Equal("File path not set for RolesLoader.", message);
        }

        [Fact]
        public async void Can_Load_Model_With_Fluent_Api()
        {
            // Arrange
            var fileParser = new Mock<IFileLoader>();
            var users = new List<User> { new User() };
            fileParser.Setup(r => r.LoadFile<User>(It.IsAny<string>())).ReturnsAsync(users);
            var userRepository = new Mock<IUserRepository>();
            var userResult = new User().ToOperationResult();
            userRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<IDbContextTransaction>(), It.IsAny<UserInfo>())).ReturnsAsync(userResult);
            var settings = new InitialDataSettingsSeam();
            var userLoader = new UsersLoader(fileParser.Object, settings, userRepository.Object);
            var contentRootPath = "C:/My/Path";
            // Act
            var opResult = await userLoader.ConfigureInitialDataAsync(contentRootPath);

            // Assert
            Assert.True(opResult.AnySucceeded);
            userRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<IDbContextTransaction>(), It.IsAny<UserInfo>()), Times.Once);
        }

        [Fact]
        public async void Cant_Load_Existing_Model_With_Fluent_Api()
        {
            // Arrange
            var fileParser = new Mock<IFileLoader>();
            var users = new List<User> { new User() };
            fileParser.Setup(r => r.LoadFile<User>(It.IsAny<string>())).ReturnsAsync(users);
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(r => r.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new User());
            var settings = new InitialDataSettingsSeam();
            var userLoader = new UsersLoader(fileParser.Object, settings, userRepository.Object);
            var contentRootPath = "C:/My/Path";

            // Act
            var opResult = await userLoader.ConfigureInitialDataAsync(contentRootPath);

            // Assert
            Assert.False(opResult.AnySucceeded);

            var errorMessage = opResult.Errors.ToRawString();
            Assert.Contains("This model was already added.", errorMessage);
        }

        [Fact]
        [Trait("Unstable", "True")]
        public async void Can_LoadModelWithCollaborators()
        {
            // Arrange
            var batchResult = new BatchOperationResult<PrivilegeProfileRole>();
            using (var context = GetContext("Can_Override_Model"))
            {
                var repository = GetRoleProfileRepository(context);
                var loader = GetRoleProfilesLoader();
                var collaborators = GetProfileRoleCollaborators();

                // Act
                batchResult = await loader.ConfigureInitialDataAsync("my/path", collaborators);
            }

            // Assert
            Assert.True(batchResult.AllSucceeded);
            var roleProfile1 = batchResult.Payloads.ElementAt(0);
            var roleProfile2 = batchResult.Payloads.ElementAt(1);
            Assert.Equal(2, batchResult.Payloads.Count());

            Assert.Equal(2, roleProfile1.ProfileId);

            Assert.Equal(2, roleProfile2.RoleId);
            Assert.Equal(1, roleProfile2.ProfileId);
        }

        private RoleProfilesTestLoader GetRoleProfilesLoader(IRoleProfileRepository repository = null)
        {
            var fileParser = new Mock<IFileLoader>();
            var profileRoles = GetProfileRoles();
            var roleProfileRepository = new Mock<IRoleProfileRepository>();
            roleProfileRepository.SetupCreateAsync<int, IRoleProfileRepository, PrivilegeProfileRole>(new PrivilegeProfileRole());
            repository = repository ?? roleProfileRepository.Object;
            fileParser.Setup(r => r.LoadFile<PrivilegeProfileRole>(It.IsAny<string>())).ReturnsAsync(profileRoles);
            var settings = new InitialDataSettingsSeam();
            var mapper = MapperFactory.GetMapper();
            return new RoleProfilesTestLoader(fileParser.Object, settings);
        }

        private IRoleProfileRepository GetRoleProfileRepository(GTHApiDb context)
        {
            var mapper = MapperFactory.GetMapper();
            var fakeTransactionProvider = new FakeTransactionProvider();
            return new RoleProfileRepository(context, mapper, fakeTransactionProvider);
        }

        private GTHApiDb GetContext(string setName)
        {
            var context = InMemoryContextFactory.GetInMemoryContext<GTHApiDb>(setName, (options) =>
            {
                using (var ctx = new GTHApiDb(options))
                {
                    if (!ctx.Roles.Any())
                    {
                        ctx.Roles.AddRange(GetRoles());
                    }
                    if (!ctx.Profiles.Any())
                    {
                        ctx.Profiles.AddRange(GetProfiles());
                    }
                    ctx.SaveChanges();
                }
            });
            return context;
        }

        private Dictionary<string, object> GetProfileRoleCollaborators()
        {
            var collaborators = new Dictionary<string, object>();
            var roles = GetRoles().Select(r => (object)r);
            var profiles = GetProfiles().Select(p => (object)p);
            collaborators.Add(InitialDataModel.Roles.ToString(), roles);
            collaborators.Add(InitialDataModel.Profiles.ToString(), profiles);
            return collaborators;
        }

        private List<PrivilegeProfileRole> GetProfileRoles()
        {
            return new List<PrivilegeProfileRole>
            {
                new PrivilegeProfileRole{
                    Role = new Role{
                        Name = "Test-Role-1"
                    },
                    Profile = new PrivilegeProfile{
                        Name = "Test-Profile-2"
                    }
                },
                new PrivilegeProfileRole{
                    Role = new Role{
                        Name = "Test-Role-2"
                    },
                    Profile = new PrivilegeProfile{
                        Name = "Test-Profile-1"
                    }
                }
            };
        }

        private List<Role> GetRoles()
        {
            return new List<Role>
            {
                new Role{
                    Id = 1,
                    Name = "Test-Role-1",
                    IsEnabled = true,
                },
                new Role{
                    Id = 2,
                    Name = "Test-Role-2",
                    IsEnabled = true
                }
            };
        }

        private static List<PrivilegeProfile> GetProfiles()
        {
            return new List<PrivilegeProfile>{
                new PrivilegeProfile{
                    Id = 1,
                    Name = "Test-Profile-1",
                    IsEnabled = true
                },
                new PrivilegeProfile{
                    Id = 2,
                    Name = "Test-Profile-2",
                    IsEnabled = true
                }
            };
        }


    }

}