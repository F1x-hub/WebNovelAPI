using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Dto.Coins;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class MonetizationController : ControllerBase
    {
        private readonly IAuthorMonetizationService _monetizationService;
        private readonly BasicWebNovelContext _context;

        public MonetizationController(IAuthorMonetizationService monetizationService, BasicWebNovelContext context)
        {
            _monetizationService = monetizationService;
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        private async Task VerifyAuthorAsync(int novelId)
        {
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
            {
                throw new NotFoundException("Novel not found");
            }

            bool isAdmin = User.IsInRole("Admin");
            if (novel.UserId != CurrentUserId && !isAdmin)
            {
                throw new BadRequestException("You are not authorized to manage monetization for this novel");
            }
        }

        /// <summary>
        /// Retrieves the monetization settings for a specific novel
        /// </summary>
        [HttpGet("{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPricing(int novelId)
        {
            await VerifyAuthorAsync(novelId);
            var pricing = await _monetizationService.GetPricingAsync(novelId);
            return Ok(pricing);
        }

        /// <summary>
        /// Saves or updates the monetization settings for a specific novel
        /// </summary>
        [HttpPut("{novelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SavePricing(int novelId, [FromBody] UpdatePricingRequest request)
        {
            await VerifyAuthorAsync(novelId);
            var pricing = await _monetizationService.SavePricingAsync(CurrentUserId, novelId, request);
            return Ok(pricing);
        }
    }
}
