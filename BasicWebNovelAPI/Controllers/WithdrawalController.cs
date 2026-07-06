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
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WithdrawalController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly BasicWebNovelContext _context;

        public WithdrawalController(IWithdrawalService withdrawalService, BasicWebNovelContext context)
        {
            _withdrawalService = withdrawalService;
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        private async Task VerifyAuthorAsync()
        {
            bool isAdmin = User.IsInRole("Admin");
            bool isAuthor = await _context.Novels.AnyAsync(n => n.UserId == CurrentUserId);
            if (!isAuthor && !isAdmin)
            {
                throw new BadRequestException("Only authors can request withdrawals or view withdrawal history");
            }
        }

        /// <summary>
        /// Retrieves the withdrawal history for the current author
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWithdrawals()
        {
            await VerifyAuthorAsync();
            var withdrawals = await _withdrawalService.GetWithdrawalsAsync(CurrentUserId);
            return Ok(withdrawals);
        }

        /// <summary>
        /// Submits a new withdrawal request for the current author
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequest request)
        {
            await VerifyAuthorAsync();
            var withdrawal = await _withdrawalService.RequestWithdrawalAsync(CurrentUserId, request.CoinsAmount);
            return Ok(withdrawal);
        }
    }

    public class WithdrawalRequest
    {
        public int CoinsAmount { get; set; }
    }
}
