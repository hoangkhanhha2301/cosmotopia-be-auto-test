using AutoMapper;
using ComedicShopAPI.Controllers;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.Service.OTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmetics.Tests
{
    public class UserPasswordControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IOptionsMonitor<AppSetting>> _mockAppSetting;
        private readonly UserController _controller;

        public UserPasswordControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEmailService = new Mock<IEmailService>();
            _mockMapper = new Mock<IMapper>();
            _mockAppSetting = new Mock<IOptionsMonitor<AppSetting>>();
            _mockAppSetting.Setup(x => x.CurrentValue).Returns(new AppSetting { SecretKey = "TestKey" });

            _controller = new UserController(null, _mockUnitOfWork.Object, _mockAppSetting.Object, _mockMapper.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsError_WhenEmailNotFound()
        {
            _mockUnitOfWork.Setup(x => x.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                           .ReturnsAsync((User)null);

            var model = new ForgotPasswordModel { Email = "notfound@example.com" };
            var result = await _controller.ForgotPassword(model);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid Email", response.Message);
        }

        [Fact]
        public async Task ResetPassword_ReturnsError_WhenPasswordMismatch()
        {
            var model = new SetNewPasswordModel
            {
                NewPassword = "123456",
                ConfirmPassword = "654321",
                Token = "token123"
            };

            var result = await _controller.ResetPassword(model);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(bad.Value);
            Assert.False(response.Success);
            Assert.Equal("Password and confirm password do not match", response.Message);
        }

        [Fact]
        public async Task ResetPassword_ReturnsError_WhenTokenInvalid()
        {
            var model = new SetNewPasswordModel
            {
                NewPassword = "123456",
                ConfirmPassword = "123456",
                Token = "invalid-token"
            };

            _mockUnitOfWork.Setup(x => x.Users.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()!))
                           .ReturnsAsync(new List<User>());

            var result = await _controller.ResetPassword(model);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(bad.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid or expired token", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsError_WhenOldPasswordWrong()
        {
            var model = new ChangePasswordModel
            {
                Email = "user@example.com",
                OldPassword = "wrong",
                NewPassword = "newpass"
            };

            _mockUnitOfWork.Setup(x => x.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                           .ReturnsAsync(new User { Email = model.Email, Password = BCrypt.Net.BCrypt.HashPassword("correct") });

            var result = await _controller.ChangePassword(model);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid Email or Old Password. Please try again", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsSuccess_WhenValid()
        {
            var user = new User { Email = "user@example.com", Password = BCrypt.Net.BCrypt.HashPassword("oldpass") };
            var model = new ChangePasswordModel
            {
                Email = user.Email,
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };

            _mockUnitOfWork.Setup(x => x.Users.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                           .ReturnsAsync(user);

            var result = await _controller.ChangePassword(model);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal("Password changed successfully", response.Message);
        }
    }
}
