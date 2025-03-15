using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
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

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    AffiliateProfileId = order.AffiliateProfileId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDTO dto)
        {
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                SalesStaffId = dto.SalesStaffId,
                AffiliateProfileId = dto.AffiliateProfileId,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                OrderDate = dto.OrderDate ?? DateTime.UtcNow,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentStatus
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
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
            order.AffiliateProfileId = dto.AffiliateProfileId;
            order.TotalAmount = dto.TotalAmount;
            order.Status = dto.Status;
            order.OrderDate = dto.OrderDate;
            order.PaymentMethod = dto.PaymentMethod;
            order.PaymentStatus = dto.PaymentStatus;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return NotFound();

            var responseDTO = new OrderResponseDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
                SalesStaffId = order.SalesStaffId,
                AffiliateProfileId = order.AffiliateProfileId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
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
                    AffiliateProfileId = order.AffiliateProfileId,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
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

    }

}