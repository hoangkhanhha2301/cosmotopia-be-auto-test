using Cosmetics.DTO.Affiliate;
using Cosmetics.DTO.User;
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

        [HttpPost("generate-link")]
[Authorize(Roles = "Affiliates")]
public async Task<IActionResult> GenerateLink([FromBody] GenerateLinkRequestDto request)
{
    try
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
        var link = await _affiliateService.GenerateAffiliateLinkAsync(userId, request.ProductId);

        var response = new ApiResponse
        {
            Success = true,
            StatusCode = 0,
            Message = "Link generated successfully.",
            Data = link
        };

        return Ok(response);
    }
    catch (Exception ex)
    {
        var response = new ApiResponse
        {
            Success = false,
            StatusCode = 1,
            Message = ex.Message,
            Data = null
        };
        return BadRequest(response);
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
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetStats(DateTime startDate, DateTime endDate)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var result = await _affiliateService.GetAffiliateStatsAsync(userId, startDate, endDate);
            return Ok(result);
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
            var result = await _affiliateService.RequestWithdrawalAsync(userId, request);
            return Ok(result);
        }

        [HttpPut("withdraw/{transactionId}/status")]
        [Authorize(Roles = "Manager")] // Chỉ Manager mới được cập nhật trạng thái
        public async Task<IActionResult> UpdateWithdrawalStatus(Guid transactionId, [FromBody] WithdrawalStatus status)
        {
            try
            {
                var result = await _affiliateService.UpdateWithdrawalStatusAsync(transactionId, status);

                var response = new ApiResponse
                {
                    Success = true,
                    StatusCode = 0,
                    Message = "Withdrawal status updated successfully.",
                    Data = result
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    StatusCode = 1,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
        }


        [HttpGet("profile")]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetAffiliateProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
                var profile = await _affiliateService.GetAffiliateProfileAsync(userId);

                var response = new ApiResponse
                {
                    Success = true,
                    StatusCode = 0,
                    Message = "User found",
                    Data = profile
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    StatusCode = 1,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
        }


        [HttpGet("withdrawals")]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetWithdrawalsByAffiliate()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
                var withdrawals = await _affiliateService.GetWithdrawalsByAffiliateAsync(userId);

                var response = new ApiResponse
                {
                    Success = true,
                    StatusCode = 0,
                    Message = "Withdrawals retrieved successfully.",
                    Data = withdrawals
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    StatusCode = 1,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
        }

        [HttpGet("manager/withdrawals")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllWithdrawals()
        {
            try
            {
                var withdrawals = await _affiliateService.GetAllWithdrawalsAsync();

                var response = new ApiResponse
                {
                    Success = true,
                    StatusCode = 0,
                    Message = "All withdrawals retrieved successfully.",
                    Data = withdrawals
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    StatusCode = 1,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
        }

        [HttpGet("links")]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetAllLinks()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated."));
                var links = await _affiliateService.GetAllLinksAsync(userId);

                var response = new ApiResponse
                {
                    Success = true,
                    StatusCode = 0,
                    Message = "All links retrieved successfully.",
                    Data = links
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse
                {
                    Success = false,
                    StatusCode = 1,
                    Message = ex.Message,
                    Data = null
                };
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetAllEarnings()
        {
            try
            {
                // Lấy userId từ token (giả sử bạn dùng JWT)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid user ID in token.");
                }

                // Gọi phương thức GetAllEarningsAsync từ AffiliateService
                var earnings = await _affiliateService.GetAllEarningsAsync(userId);

                return Ok(earnings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/affiliate/earnings/summary
        [HttpGet("earnings/summary")]
        [Authorize(Roles = "Affiliates")]
        public async Task<IActionResult> GetAffiliateSummary()
        {
            try
            {
                // Lấy userId từ token (Claims)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user ID in token." });
                }

                // Gọi service để lấy tổng thu nhập và tổng số lượt click
                var summary = await _affiliateService.GetAffiliateSummaryAsync(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}