using AutoMapper;
using Cosmetics.DTO.Affiliate;
using Cosmetics.Models;
using Cosmetics.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliateProfileController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AffiliateProfileController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _unitOfWork.AffiliateProfiles.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound($"Affiliate profile with ID {id} not found.");
            }
            var result = _mapper.Map<AffiliateProfileDTO>(profile);
            return Ok(result);
        }
    }
}
