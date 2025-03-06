using AutoMapper;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

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

        // Lấy tất cả OrderDetail
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync();
            var result = _mapper.Map<IEnumerable<OrderDetailDTO>>(orderDetails);
            return Ok(result);
        }

        // Lấy OrderDetail theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (orderDetail == null) return NotFound();

            var result = _mapper.Map<OrderDetailDTO>(orderDetail);
            return Ok(result);
        }

        // Tạo mới OrderDetail
        [HttpPost]
        public async Task<IActionResult> Create(OrderDetailCreateDTO orderDetailDTO)
        {
            if (orderDetailDTO == null) return BadRequest();

            var orderDetail = _mapper.Map<OrderDetail>(orderDetailDTO);
            await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<OrderDetailDTO>(orderDetail);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderDetailId }, result);
        }

        // Cập nhật OrderDetail
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, OrderDetailUpdateDTO orderDetailDTO)
        {
            if (id != orderDetailDTO.OrderDetailId) return BadRequest();

            var existingOrderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (existingOrderDetail == null) return NotFound();

            _mapper.Map(orderDetailDTO, existingOrderDetail);
            _unitOfWork.OrderDetails.Update(existingOrderDetail);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // Xóa OrderDetail
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var orderDetail = await _unitOfWork.OrderDetails.GetByIdAsync(id);
            if (orderDetail == null) return NotFound();

            _unitOfWork.OrderDetails.Delete(orderDetail);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }

}
