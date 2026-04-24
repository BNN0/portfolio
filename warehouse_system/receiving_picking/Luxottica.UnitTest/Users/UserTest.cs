using Luxottica.ApplicationServices.Shared.Dto.Users;
using Luxottica.ApplicationServices.Users;
using Luxottica.Controllers.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Luxottica.UnitTest
{
    public class UserTest
    {
        private Mock<IUserAppService> _userAppServiceMock;
        private UserController _controllerU;

        [SetUp]
        public void Setup()
        {
            _userAppServiceMock = new Mock<IUserAppService>();
            var loggerMock = new Mock<ILogger<UserController>>();
            _controllerU = new UserController(_userAppServiceMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task TestAddUser()
        {
            // Arrange
            string phoneNumber = "1234567896";
            string password = "Pa$$W0*,.";
            string email = "admin1@luxottica.com";
            Boolean EmailConfirmed = true;
            string Role = "Admin";
            string username = "UsernameTest";

            var newUser = new NewUserDto {Email = email, Password = password, PhoneNumber = phoneNumber, UserName = username};
            _userAppServiceMock.Setup(service => service.AddUserAsync(newUser));

            // Act
            await _controllerU.Post(newUser);

            // Assert
            Assert.IsNotNull(newUser);
            _userAppServiceMock.Verify(service => service.AddUserAsync(newUser), Times.Once);
        }

        [Test]
        public async Task TestGetAllUser()
        {
            // Arrange
            var usersIdentity = new List<UserDto>
            {
                new UserDto {
                    Id = "userid1",
                    Email = "admin1@luxottica.com",
                    PhoneNumber = "1234567896",
                    UserName = "admin1@luxottica.com",
                },
                new UserDto {
                    Id = "userid2",
                    Email = "admin2@luxottica.com",
                    PhoneNumber = "1234567899",
                    UserName = "admin2@luxottica.com",
                }
            };
            _userAppServiceMock.Setup(service => service.GetUsersAsync())
                .ReturnsAsync(usersIdentity);

            // Act
            var result = await _controllerU.Get();

            // Assert
            var users = result.ExecuteResultAsync;
            Assert.NotNull(users);
        }

        [Test]
        public async Task TestGetByIdUser()
        {
            // Arrange
            string testId = "id1";
            var userIdentity = new UserDto
            {
                Id = testId,
                Email = "admin1@luxottica.com",
                PhoneNumber = "1234567895",
                UserName = "admin1@luxottica.com"
            };
            _userAppServiceMock.Setup(service => service.GetUserAsync(testId))
                .ReturnsAsync(userIdentity);

            // Act
            var result = await _controllerU.Get(testId);


            var user = result.Equals(userIdentity);
            Assert.NotNull(user);
            Assert.AreEqual(testId, userIdentity.Id);
        }
        [Test]
        public async Task TestEditUser()
        {
            // Arrange
            string testId = "id1";
            var phoneNumber = "1478963251";
            var username = "adminupdate@luxottica.com";
            var editeduser = new EditUserDto {PhoneNumber = phoneNumber, UserName = username};
            _userAppServiceMock.Setup(service => service.EditUserAsync(testId, editeduser));

            // Act
            await _controllerU.Put(testId, editeduser);

            // Assert
            _userAppServiceMock.Verify(service => service.EditUserAsync(testId, editeduser), Times.Once);
        }

        //[Test]
        //public async Task TestDeleteUser()
        //{
        //    // Arrange
        //    string testId = "id1";
        //    _userAppServiceMock.Setup(service => service.DeleteUserAsync(testId));

        //    // Act
        //    await _controllerU.Delete(testId);

        //    // Assert
        //    _userAppServiceMock.Verify(service => service.DeleteUserAsync(testId), Times.Once);
        //}
    }
}