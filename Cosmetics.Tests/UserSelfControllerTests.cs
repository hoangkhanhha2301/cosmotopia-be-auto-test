using AutoMapper;
using ComedicShopAPI.Controllers;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Cosmetics.Service.OTP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cosmetics.Tests
{
    public class UserSelfControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IOptionsMonitor<AppSetting>> _mockAppSetting;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserController _controller;

        public UserSelfControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAppSetting = new Mock<IOptionsMonitor<AppSetting>>();
            _mockEmailService = new Mock<IEmailService>();

            _mockAppSetting.Setup(x => x.CurrentValue).Returns(new AppSetting { SecretKey = "SecretKey" });
            _controller = new UserController(null, _mockUnitOfWork.Object, _mockAppSetting.Object, _mockMapper.Object, _mockEmailService.Object);
        }

        private void SetUserContext(int userId, string role = "Customers")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsUnauthorized_WhenNoUserId()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetCurrentUser();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(unauthorized.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid user ID", response.Message);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsNotFound_WhenUserNotExist()
        {
            SetUserContext(99);
            _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(99)).ReturnsAsync((User)null);

            var result = await _controller.GetCurrentUser();

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(notFound.Value);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsSuccess_WhenUserExists()
        {
            SetUserContext(1);
            var user = new User { UserId = 1, FirstName = "Tuan" };
            var dto = new UserDTO { FirstName = "Tuan" };

            _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1)).ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserDTO>(user)).Returns(dto);

            var result = await _controller.GetCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal("Tuan", ((UserDTO)response.Data).FirstName);
        }

        [Fact]
        public async Task EditSelf_ReturnsBadRequest_WhenAffiliate()
        {
            SetUserContext(1, "Affiliates");

            var model = new EditSelfModel { FirstName = "A", LastName = "B", Phone = "0123456" };
            var result = await _controller.EditSelf(model);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(bad.Value);
            Assert.False(response.Success);
            Assert.Equal("Affiliates are not allowed to edit their profile.", response.Message);
        }

        [Fact]
        public async Task EditSelf_ReturnsSuccess_WhenValid()
        {
            SetUserContext(1);
            var model = new EditSelfModel { FirstName = "New", LastName = "User", Phone = "0123" };
            var user = new User { UserId = 1, FirstName = "Old" };

            _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1)).ReturnsAsync(user);
            _mockUnitOfWork.Setup(x => x.Users.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>())).ReturnsAsync(false);

            var result = await _controller.EditSelf(model);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(ok.Value);
            Assert.True(response.Success);
            Assert.Equal("User updated successfully", response.Message);
        }
    }
}
