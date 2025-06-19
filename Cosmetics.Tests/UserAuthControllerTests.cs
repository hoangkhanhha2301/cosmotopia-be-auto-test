using AutoMapper;
using ComedicShopAPI.Controllers;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.Service.OTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace Cosmetics.Tests
{
    public class UserAuthControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IOptionsMonitor<AppSetting>> _mockAppSetting;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserController _controller;

        public UserAuthControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAppSetting = new Mock<IOptionsMonitor<AppSetting>>();
            _mockEmailService = new Mock<IEmailService>();

            _mockAppSetting.Setup(x => x.CurrentValue).Returns(new AppSetting { SecretKey = "TestSecretKey" });
            _controller = new UserController(null, _mockUnitOfWork.Object, _mockAppSetting.Object, _mockMapper.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task Login_ReturnsError_WhenEmailNotExist()
        {
            var model = new LoginModel { Email = "notfound@example.com", Password = "123456" };
            _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>())).ReturnsAsync((User)null);

            var result = await _controller.Validate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid Username/Password", response.Message);
        }

        [Fact]
        public async Task Login_ReturnsError_WhenPasswordIncorrect()
        {
            var user = new User { Email = "user@example.com", Password = "hashed", Verify = 4, UserStatus = 0, RoleType = 3 };
            var model = new LoginModel { Email = "user@example.com", Password = "wrongpassword" };

            _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>())).ReturnsAsync(user);

            var result = await _controller.Validate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid Username/Password", response.Message);
        }

        [Fact]
        public async Task Login_ReturnsError_WhenUserNotVerified()
        {
            var user = new User { Email = "user@example.com", Password = "123456", Verify = 0, UserStatus = 0, RoleType = 3 };
            var model = new LoginModel { Email = "user@example.com", Password = "123456" };

            _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>())).ReturnsAsync(user);

            var result = await _controller.Validate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Account is not verified. Please verify your account", response.Message);
        }

        [Fact]
        public async Task Login_ReturnsError_WhenUserLocked()
        {
            var user = new User { Email = "user@example.com", Password = "123456", Verify = 4, UserStatus = 1, RoleType = 3 };
            var model = new LoginModel { Email = "user@example.com", Password = "123456" };

            _mockUnitOfWork.Setup(u => u.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>())).ReturnsAsync(user);

            var result = await _controller.Validate(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Account is locked. Please contact support for assistance.", response.Message);
        }
    }
}
