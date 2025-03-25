using Cosmetics.DTO.Affiliate;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Service.Affiliate.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cosmetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliateController : ControllerBase
    {
        private readonly IAffiliateService _affiliateService;

        public AffiliateController(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAffiliate([FromBody] AffiliateRegistrationRequestDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
            var result = await _affiliateService.RegisterAffiliateAsync(userId, request);
            return Ok(result);
        }

        [HttpPost("create-link")]
        public async Task<IActionResult> CreateAffiliateLink([FromQuery] Guid productId)
        {
            try
            {
                // Ensure the user is authenticated and has a valid userId
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    // If userId is missing or invalid, return Unauthorized or BadRequest
                    return Unauthorized(new { message = "User is not authenticated or invalid user ID." });
                }

                // Call the service method to generate the affiliate link
                var result = await _affiliateService.GenerateAffiliateLinkAsync(userId, productId);

                // Return the generated affiliate link DTO in a successful response
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Catch any exceptions and return a bad request with the error message
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("track-click")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackClick([FromQuery] string referralCode)
        {
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim);
                }
            }

            await _affiliateService.TrackAffiliateClickAsync(referralCode, userId);
            return Ok(new { Message = "Click tracked successfully" });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(DateTime startDate, DateTime endDate)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var result = await _affiliateService.GetAffiliateStatsAsync(userId, startDate, endDate);
            return Ok(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto request)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var result = await _affiliateService.RequestWithdrawalAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("withdraw/{transactionId}/status")]
        [Authorize(Roles = "Manager")] // Chỉ Manager mới được cập nhật trạng thái
        public async Task<IActionResult> UpdateWithdrawalStatus(Guid transactionId, [FromBody] WithdrawalStatus status)
        {
            var result = await _affiliateService.UpdateWithdrawalStatusAsync(transactionId, status);
            return Ok(result);
        }
    }
}