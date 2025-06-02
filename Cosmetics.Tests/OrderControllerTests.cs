using Cosmetics.Controllers;
using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System.Security.Claims;

namespace Cosmetics.Tests
{
    public class OrderControllerTests
    {
        private readonly DbContextOptions<ComedicShopDBContext> _dbOptions;
        private readonly ComedicShopDBContext _context;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ComedicShopDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ComedicShopDBContext(_dbOptions);
            _context.Database.EnsureCreated();

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new OrderController(_mockUnitOfWork.Object, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult_WhenValidRequest()
        {
            // Arrange
            _context.Orders.AddRange(new List<Order>
            {
                new Order { OrderId = Guid.NewGuid(), CustomerId = 1, TotalAmount = 100, Status = OrderStatus.Pending, OrderDate = DateTime.UtcNow },
                new Order { OrderId = Guid.NewGuid(), CustomerId = 2, TotalAmount = 200, Status = OrderStatus.Paid, OrderDate = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOrders(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Use System.Text.Json to parse anonymous object
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            int totalCount = root.GetProperty("TotalCount").GetInt32();
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtActionResult_WhenValidOrder()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _context.Products.Add(new Product { ProductId = productId, Name = "Product A", Price = 50 });
            await _context.SaveChangesAsync();

            var orderDto = new OrderCreateDTO
            {
                SalesStaffId = 1,
                PaymentMethod = "Credit Card",
                Address = "123 Main St",
                OrderDetails = new List<OrderDetailCreateDTO>
                {
                    new OrderDetailCreateDTO { ProductId = productId, Quantity = 2 }
                }
            };

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier))
                    .Returns(new Claim(ClaimTypes.NameIdentifier, "1"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockUser.Object }
            };

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetOrder", createdResult.ActionName);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNoContent_WhenValidUpdate()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, Status = OrderStatus.Paid };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderUpdateDto = new OrderUpdateDTO { OrderId = orderId, Status = OrderStatus.Shipped };

            // Act
            var result = await _controller.UpdateOrder(orderId, orderUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedOrder = await _context.Orders.FindAsync(orderId);
            Assert.Equal(OrderStatus.Shipped, updatedOrder!.Status);
        }
    }
}
