using AutoMapper;
using Cosmetics.Controllers;
using Cosmetics.DTO.Product;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Cosmetics.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _controller = new ProductController(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task DeleteById_ReturnsNotFound_WhenProductIsNull()
        {
            _mockUnitOfWork.Setup(x => x.Products.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            var result = await _controller.DeleteById(Guid.NewGuid());

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(okResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Product does not exist!", response.Message);
        }

        [Fact]
        public async Task Create_ReturnsError_WhenCategoryOrBrandNotExist()
        {
            var dto = new ProductCreateDTO
            {
                Name = "Test",
                Description = "Test",
                Price = 10,
                StockQuantity = 5,
                CommissionRate = 0.1m,
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                imageUrls = new List<string>()
            };

            _mockUnitOfWork.Setup(x => x.Products.CategoryExist(dto.CategoryId.Value)).ReturnsAsync(false);

            var result = await _controller.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(badRequest.Value);
            Assert.False(response.Success);
            Assert.Equal("Category does not exist!", response.Message);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenProductDoesNotExist()
        {
            _mockUnitOfWork.Setup(x => x.Products.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            var dto = new ProductUpdateDTO();
            var result = await _controller.Update(Guid.NewGuid(), dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(notFound.Value);
            Assert.False(response.Success);
            Assert.Equal("Product not found!", response.Message);
        }

        [Fact]
        public async Task Create_ReturnsSuccess_WhenValid()
        {
            var dto = new ProductCreateDTO
            {
                Name = "Test",
                Description = "Test",
                Price = 100,
                StockQuantity = 10,
                BrandId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                imageUrls = new List<string>(),
                CommissionRate = 0.1m
            };

            _mockUnitOfWork.Setup(x => x.Products.CategoryExist(dto.CategoryId.Value)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.Products.BranchExist(dto.BrandId.Value)).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<ProductDTO>(It.IsAny<Product>())).Returns(new ProductDTO { Name = dto.Name });

            var result = await _controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Created Product Successfully.", response.Message);
        }
    }
}
