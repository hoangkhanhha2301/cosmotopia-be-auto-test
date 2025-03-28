using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            // Lấy tất cả orders và include các bảng liên quan
            var ordersQuery = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .AsQueryable();

            var totalCount = await ordersQuery.CountAsync();

            var paginatedOrders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}".Trim(),
                    PhoneNumber = order.Customer.Phone,
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
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ImageUrl = od.Product.ImageUrls,
                        AffiliateProfileId = od.AffiliateProfileId
                    }).ToList()
                })
                .ToListAsync();

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

            // Extract UserID from the authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User is not authenticated.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Invalid user ID.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        CustomerId = userId, // Maps to UserID in Users table (int)
                        SalesStaffId = dto.SalesStaffId,
                        TotalAmount = 0,
                        Status = OrderStatus.Pending,
                        OrderDate = DateTime.UtcNow,
                        PaymentMethod = dto.PaymentMethod,
                        Address = dto.Address
                    };

                    var orderDetails = new List<OrderDetail>();
                    foreach (var detailDto in dto.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(detailDto.ProductId.Value);
                        if (product == null)
                            return NotFound($"Product with ID {detailDto.ProductId} not found.");

                        if (product.StockQuantity < detailDto.Quantity)
                            return BadRequest($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {detailDto.Quantity}");

                        product.StockQuantity -= detailDto.Quantity;
                        _context.Products.Update(product);

                        decimal unitPrice = product.Price;
                        decimal commissionRate = product.CommissionRate / 100 ?? 0;
                        decimal commissionAmount = commissionRate * unitPrice * detailDto.Quantity;

                        // Lấy AffiliateProfileId trực tiếp từ database
                        Guid? affiliateProfileId = null;
                        var currentTime = DateTime.UtcNow;
                        var expiryTime = currentTime.AddDays(-7);

                        // Tìm ClickTracking mới nhất của userId, trong vòng 7 ngày
                        var latestClick = await _context.ClickTrackings
                            .Where(ct => ct.UserId == userId && ct.ClickedAt >= expiryTime)
                            .OrderByDescending(ct => ct.ClickedAt)
                            .FirstOrDefaultAsync();

                        if (latestClick != null)
                        {
                            // Tìm AffiliateLink dựa trên ReferralCode và ProductID
                            var affiliateLink = await _context.AffiliateProductLinks
                                .Where(al => al.ReferralCode == latestClick.ReferralCode && al.ProductId == detailDto.ProductId.Value)
                                .FirstOrDefaultAsync();

                            if (affiliateLink != null)
                            {
                                affiliateProfileId = affiliateLink.AffiliateProfileId;
                            }
                            else
                            {
                                Debug.WriteLine($"No AffiliateLink found for ReferralCode: {latestClick.ReferralCode} and ProductID: {detailDto.ProductId}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"No valid ClickTracking found for UserID: {userId} within 7 days");
                        }

                        orderDetails.Add(new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = detailDto.ProductId.Value,
                            Quantity = detailDto.Quantity,
                            UnitPrice = unitPrice,
                            CommissionAmount = commissionAmount,
                            AffiliateProfileId = affiliateProfileId // Gán AffiliateProfileId
                        });
                    }

                    order.TotalAmount = orderDetails.Sum(od => od.Quantity * od.UnitPrice);

                    _context.Orders.Add(order);
                    _context.OrderDetails.AddRange(orderDetails);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderUpdateDTO dto)
        {
            if (id != dto.OrderId) return BadRequest();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // Kiểm tra trạng thái hợp lệ: chỉ cho phép chuyển từ Paid (1) sang Shipped (2), hoặc Shipped (2) sang Delivered (3)
            if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Shipped)
            {
                return BadRequest($"Order status can only be updated from Paid (1) to Shipped (2) or Shipped (2) to Delivered (3). Current status: {order.Status}");
            }

            if (dto.Status == OrderStatus.Shipped)
            {
                if (order.Status != OrderStatus.Paid)
                    return BadRequest($"Order status can only be updated from Paid (1) to Shipped (2). Current status: {order.Status}");
            }
            else if (dto.Status == OrderStatus.Delivered)
            {
                if (order.Status != OrderStatus.Shipped)
                    return BadRequest($"Order status can only be updated from Shipped (2) to Delivered (3). Current status: {order.Status}");
            }
            else
            {
                return BadRequest($"Invalid status update. Only updates to Shipped (2) or Delivered (3) are allowed.");
            }

            // Cập nhật trạng thái
            order.Status = dto.Status;

            // Nếu trạng thái mới là Delivered, xử lý commission (bỏ qua nếu affiliate null)
            if (order.Status == OrderStatus.Delivered)
            {
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == order.OrderId)
                    .ToListAsync();

                if (orderDetails.Any())
                {
                    foreach (var detail in orderDetails)
                    {
                        if (detail.AffiliateProfileId.HasValue)
                        {
                            var affiliateProfile = await _context.AffiliateProfiles
                                .FindAsync(detail.AffiliateProfileId.Value);

                            if (affiliateProfile != null)
                            {
                                affiliateProfile.TotalEarnings += detail.CommissionAmount;
                                affiliateProfile.Ballance += detail.CommissionAmount;
                                _context.AffiliateProfiles.Update(affiliateProfile);
                            }
                        }
                    }
                }
            }

            // Cập nhật Order bất kể affiliate có null hay không
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

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
                PhoneNumber = order.Customer?.Phone,
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
        public async Task<IActionResult> GetOrdersByUser([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
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
                    PhoneNumber = order.Customer?.Phone,
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
        //[HttpGet("shipper-confirmed-paid")]
        //[Authorize(Roles = "Shipper")]
        //public async Task<IActionResult> GetConfirmedPaidOrdersForShipper([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        //{
        //    if (page < 1 || pageSize < 1)
        //    {
        //        return BadRequest("Page and pageSize must be greater than 0.");
        //    }

        //    var orders = await _unitOfWork.Orders.GetConfirmedPaidOrdersForShipperAsync(page, pageSize);
        //    if (!orders.Any())
        //    {
        //        return NotFound("No confirmed and paid orders found for shipping.");
        //    }

        //    var totalCount = await _unitOfWork.Orders.CountAsync(
        //        filter: o => o.Status == OrderStatus.Confirmed
        //                  && _context.PaymentTransactions.Any(pt => pt.OrderId == o.OrderId && pt.Status == PaymentStatus.Success)
        //    );

        //    var paginatedOrders = orders.Select(order => new OrderResponseDTO
        //    {
        //        OrderId = order.OrderId,
        //        CustomerId = order.CustomerId,
        //        CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim(),
        //        SalesStaffId = order.SalesStaffId,
        //        TotalAmount = order.TotalAmount,
        //        Status = order.Status,
        //        OrderDate = order.OrderDate,
        //        PaymentMethod = order.PaymentMethod,
        //        Address = order.Address,
        //        OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDTO
        //        {
        //            OrderDetailId = od.OrderDetailId,
        //            OrderId = od.OrderId,
        //            ProductId = od.ProductId,
        //            Name = od.Product.Name,
        //            ImageUrl = od.Product.ImageUrls,
        //            Quantity = od.Quantity,
        //            UnitPrice = od.UnitPrice
        //        }).ToList() ?? new List<OrderDetailDTO>()
        //    }).ToList();

        //    var response = new
        //    {
        //        TotalCount = totalCount,
        //        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        //        CurrentPage = page,
        //        PageSize = pageSize,
        //        Orders = paginatedOrders
        //    };

        //    return Ok(response);
        //}


    }

}