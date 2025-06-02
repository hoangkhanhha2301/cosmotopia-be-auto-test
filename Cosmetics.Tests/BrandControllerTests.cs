using AutoMapper;
using Cosmetics.Controllers;
using Cosmetics.DTO.Brand;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Cosmetics.Tests
{
    public class BrandControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly BrandController _controller;

        public BrandControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new BrandController(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetBrands_ReturnsOkResult_WithPagination()
        {
            // Arrange
            var brands = new List<Brand>
            {
                new Brand { BrandId = Guid.NewGuid(), Name = "Brand A" },
                new Brand { BrandId = Guid.NewGuid(), Name = "Brand B" },
            };
            _mockUnitOfWork.Setup(x => x.Brands.GetAllAsync()).ReturnsAsync(brands);
            _mockMapper.Setup(m => m.Map<List<BrandDTO>>(It.IsAny<List<Brand>>()))
                .Returns(brands.Select(b => new BrandDTO { Name = b.Name }).ToList());

            // Act
            var result = await _controller.GetBrands();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenBrandDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Brands.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Brand)null);

            // Act
            var result = await _controller.GetById(Guid.NewGuid());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse>(badRequest.Value);
            Assert.Equal("Brand not found!", response.Message);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenBrandIsValid()
        {
            // Arrange
            var dto = new BrandCreateDTO { Name = "New Brand", IsPremium = true };
            _mockUnitOfWork.Setup(x => x.Brands.brandNameExist(dto.Name)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<BrandDTO>(It.IsAny<Brand>())).Returns(new BrandDTO { Name = dto.Name });

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("success", okResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Delete_ReturnsForbidden_WhenBrandHasProducts()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            _mockUnitOfWork.Setup(x => x.Brands.brandHasProducts(brandId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(brandId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("products", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Update_ReturnsSuccess_WhenBrandExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new BrandUpdateDTO { Name = "Updated Brand", IsPremium = false };
            var existing = new Brand { BrandId = id, Name = "Old", IsPremium = true };
            _mockUnitOfWork.Setup(x => x.Brands.GetByIdAsync(id)).ReturnsAsync(existing);
            _mockMapper.Setup(m => m.Map<BrandDTO>(It.IsAny<Brand>())).Returns(new BrandDTO { Name = dto.Name });

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("updated", okResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
