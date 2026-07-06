using System;
using System.Threading.Tasks;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Coins;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace BasicWebNovelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CoinsController : ControllerBase
    {
        private readonly ICoinService _coinService;
        private readonly BasicWebNovelContext _context;
        private readonly IChapterAccessService _chapterAccessService;

        public CoinsController(
            ICoinService coinService, 
            BasicWebNovelContext context,
            IChapterAccessService chapterAccessService)
        {
            _coinService = coinService;
            _context = context;
            _chapterAccessService = chapterAccessService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        /// <summary>
        /// Retrieves the list of available coin packages
        /// </summary>
        [HttpGet("packages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _context.CoinPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.CoinsAmount)
                .ToListAsync();
            return Ok(packages);
        }

        /// <summary>
        /// Retrieves the current user's wallet balance and transaction history
        /// </summary>
        [HttpGet("wallet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWallet()
        {
            var wallet = await _coinService.GetWalletAsync(CurrentUserId);
            return Ok(wallet);
        }

        /// <summary>
        /// Initiates a purchase by creating a Stripe PaymentIntent
        /// </summary>
        [HttpPost("purchase")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePurchaseIntent([FromBody] PurchaseRequest request)
        {
            var intent = await _coinService.CreatePurchaseIntentAsync(
                CurrentUserId, 
                request.PackageId ?? 0, 
                request.CustomAmount);
            return Ok(intent);
        }

        /// <summary>
        /// Unlocks a chapter for the current user using coins
        /// </summary>
        [HttpPost("unlock/{chapterId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlockChapter(int chapterId)
        {
            var result = await _coinService.SpendCoinsAsync(CurrentUserId, chapterId);
            return Ok(new { Success = result });
        }

        /// <summary>
        /// Retrieves access status and info for a chapter
        /// </summary>
        [HttpGet("access/{chapterId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChapterAccessStatus(int chapterId)
        {
            var status = await _chapterAccessService.GetChapterAccessStatusAsync(CurrentUserId, chapterId);
            return Ok(status);
        }

        /// <summary>
        /// Dev/Test only: Adds free coins to the current user's wallet
        /// </summary>
        [HttpPost("add-test-coins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddTestCoins([FromBody] AddTestCoinsRequest request)
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == CurrentUserId);
            
            if (wallet == null)
            {
                wallet = new BasicWebNovelAPI.Model.Coins.UserWallet
                {
                    UserId = CurrentUserId,
                    Balance = 0,
                    TotalEarned = 0
                };
                _context.UserWallets.Add(wallet);
            }

            wallet.Balance += request.Amount;

            var transaction = new BasicWebNovelAPI.Model.Coins.CoinTransaction
            {
                UserId = CurrentUserId,
                Amount = request.Amount,
                Type = BasicWebNovelAPI.Enum.CoinTransactionType.Purchase,
                CreatedAt = DateTime.UtcNow
            };
            _context.CoinTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Ok(new { NewBalance = wallet.Balance });
        }
    }

    public class PurchaseRequest
    {
        public int? PackageId { get; set; }
        public int? CustomAmount { get; set; }
    }

    public class AddTestCoinsRequest
    {
        public int Amount { get; set; }
    }
}
