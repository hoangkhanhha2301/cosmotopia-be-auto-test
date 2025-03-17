using AutoMapper;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderDetailController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Lấy tất cả OrderDetail với phân trang
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync();
            var totalCount = orderDetails.Count();

            var paginatedOrderDetails = orderDetails
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<IEnumerable<OrderDetailDTO>>(paginatedOrderDetails);

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                OrderDetails = result
            };

            return Ok(response);
        }

        // Lấy OrderDetail theo ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (orderDetail == null)
            {
                return NotFound($"OrderDetail with ID {id} not found.");
            }

            var result = _mapper.Map<OrderDetailDTO>(orderDetail);
            return Ok(result);
        }

        // Tạo mới OrderDetail
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] OrderDetailCreateDTO orderDetailDTO)
        {
            if (orderDetailDTO == null)
            {
                return BadRequest("OrderDetail data is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderDetail = _mapper.Map<OrderDetail>(orderDetailDTO);
            orderDetail.OrderDetailId = Guid.NewGuid(); // Ensure ID is set if not in DTO

            // Optional: Validate OrderId and ProductId existence
            var orderExists = await _unitOfWork.Orders.AnyAsync(o => o.OrderId == orderDetail.OrderId);
            if (!orderExists)
            {
                return BadRequest($"Order with ID {orderDetail.OrderId} does not exist.");
            }

            await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<OrderDetailDTO>(orderDetail);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderDetailId }, result);
        }

        // Cập nhật OrderDetail
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDetailUpdateDTO orderDetailDTO)
        {
            if (orderDetailDTO == null || id != orderDetailDTO.OrderDetailId)
            {
                return BadRequest("Invalid OrderDetail data or ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingOrderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (existingOrderDetail == null)
            {
                return NotFound($"OrderDetail with ID {id} not found.");
            }

            _mapper.Map(orderDetailDTO, existingOrderDetail);
            _unitOfWork.OrderDetails.UpdateAsync(existingOrderDetail);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // Xóa OrderDetail
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var orderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (orderDetail == null)
            {
                return NotFound($"OrderDetail with ID {id} not found.");
            }

            _unitOfWork.OrderDetails.Delete(orderDetail);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}