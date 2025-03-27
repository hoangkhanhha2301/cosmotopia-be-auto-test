using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ComedicShopDBContext _context;

        public OrderController(IUnitOfWork unitOfWork, ComedicShopDBContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var orders = await _unitOfWork.Orders.GetAllAsync();
            var totalCount = orders.Count();

            var paginatedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
                    SalesStaffId = order.SalesStaffId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    PaymentMethod = order.PaymentMethod,
                    Address = order.Address,
                    OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList() ?? new List<OrderDetailDTO>()
                }).ToList();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Orders = paginatedOrders
            };

            return Ok(response);
        }

        // Ensure this is included

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO dto)
        {
            if (dto == null || dto.OrderDetails == null || !dto.OrderDetails.Any())
                return BadRequest("Order must contain at least one item.");

            // Extract UserID from the authenticated user (same as GetOrdersByUser)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User is not authenticated.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Invalid user ID.");

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        CustomerId = userId, // Maps to UserID in Users table
                        SalesStaffId = dto.SalesStaffId,
                        TotalAmount = 0,
                        Status = OrderStatus.Confirmed,
                        OrderDate = DateTime.UtcNow,
                        PaymentMethod = dto.PaymentMethod,
                        Address = dto.Address
                    };

                    var orderDetails = new List<OrderDetail>();
                    foreach (var detailDto in dto.OrderDetails)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId.Value);
                        if (product == null)
                            return NotFound($"Product with ID {detailDto.ProductId} not found.");

                        if (product.StockQuantity < detailDto.Quantity)
                            return BadRequest($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {detailDto.Quantity}");

                        product.StockQuantity -= detailDto.Quantity;
                        _unitOfWork.Products.UpdateAsync(product);

                        decimal unitPrice = product.Price;
                        decimal commissionRate = product.CommissionRate/100  ?? 0;
                        decimal commissionAmount = commissionRate *  unitPrice * detailDto.Quantity;

                        orderDetails.Add(new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = detailDto.ProductId.Value,
                            Quantity = detailDto.Quantity,
                            UnitPrice = unitPrice,
                            CommissionAmount = commissionAmount
                        });
                    }

                    order.TotalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice);

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
                    await _unitOfWork.CompleteAsync();

                    await _unitOfWork.CommitAsync(transaction);

                    return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(transaction);
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderUpdateDTO dto)
        {
            if (id != dto.OrderId) return BadRequest();

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return NotFound();

            order.CustomerId = dto.CustomerId;
            order.SalesStaffId = dto.SalesStaffId;
            order.TotalAmount = dto.TotalAmount;
            order.Status = dto.Status;
            order.OrderDate = dto.OrderDate;
            order.PaymentMethod = dto.PaymentMethod;
            order.Address = dto.Address;

            _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id, includeProperties : "OrderDetails.Product,Customer");
            if (order == null) return NotFound();

            var responseDTO = new OrderResponseDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
                SalesStaffId = order.SalesStaffId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                PaymentMethod = order.PaymentMethod,
                Address = order.Address,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    Name = od.Product.Name,
                    ImageUrl = od.Product.ImageUrls,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList()
            };

            return Ok(responseDTO);
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return NotFound();

            _unitOfWork.Orders.Delete(order);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }
        [HttpGet("user/orders")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByUser([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var orders = await _unitOfWork.Orders.GetOrdersByCustomerIdAsync(userId);
            if (!orders.Any())
            {
                return NotFound($"No orders found for user with ID {userId}.");
            }

            var totalCount = orders.Count();
            var paginatedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
                    SalesStaffId = order.SalesStaffId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    PaymentMethod = order.PaymentMethod,
                    Address = order.Address,
                    OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Name = od.Product.Name,
                        ImageUrl = od.Product.ImageUrls,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList() ?? new List<OrderDetailDTO>()
                }).ToList();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Orders = paginatedOrders
            };

            return Ok(response);
        }
        [HttpGet("shipper-confirmed-paid")]
        //[Authorize(Roles = "Shipper")]
        public async Task<IActionResult> GetConfirmedPaidOrdersForShipper([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var orders = await _unitOfWork.Orders.GetConfirmedPaidOrdersForShipperAsync(page, pageSize);
            if (!orders.Any())
            {
                return NotFound("No confirmed and paid orders found for shipping.");
            }

            var totalCount = await _unitOfWork.Orders.CountAsync(
                filter: o => o.Status == OrderStatus.Confirmed
                          && _context.PaymentTransactions.Any(pt => pt.OrderId == o.OrderId && pt.Status == PaymentStatus.Success)
            );

            var paginatedOrders = orders.Select(order => new OrderResponseDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
                SalesStaffId = order.SalesStaffId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                PaymentMethod = order.PaymentMethod,
                Address = order.Address,
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    Name = od.Product.Name,
                    ImageUrl = od.Product.ImageUrls,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList() ?? new List<OrderDetailDTO>()
            }).ToList();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Orders = paginatedOrders
            };

            return Ok(response);
        }

    }

}