using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Enum;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Coins;
using BasicWebNovelAPI.Model.Dto.Coins;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class CoinService : ICoinService
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IStripeClient _stripeClient;

        public CoinService(BasicWebNovelContext context, IMapper mapper, IStripeClient stripeClient)
        {
            _context = context;
            _mapper = mapper;
            _stripeClient = stripeClient;
        }

        public async Task<UserWalletDto> GetWalletAsync(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new NotFoundException("User not found");
            }

            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new UserWallet
                {
                    UserId = userId,
                    Balance = 0,
                    TotalEarned = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            var transactions = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var walletDto = _mapper.Map<UserWalletDto>(wallet);
            walletDto.Transactions = _mapper.Map<List<CoinTransactionDto>>(transactions);

            return walletDto;
        }

        public async Task<PaymentIntentDto> CreatePurchaseIntentAsync(int userId, int coinPackageId, int? customAmount)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new NotFoundException("User not found");
            }

            decimal priceUsd;
            int coinsAmount;

            if (customAmount.HasValue)
            {
                if (customAmount.Value < 100)
                {
                    throw new BadRequestException("Minimum custom amount is 100 coins");
                }
                coinsAmount = customAmount.Value;
                priceUsd = coinsAmount / 100.0m;
            }
            else
            {
                var pack = await _context.CoinPackages.FindAsync(coinPackageId);
                if (pack == null || !pack.IsActive)
                {
                    throw new NotFoundException("Active coin package not found");
                }

                if (pack.IsCustom)
                {
                    throw new BadRequestException("Custom amount must be provided for the custom package");
                }

                coinsAmount = pack.CoinsAmount;
                priceUsd = pack.PriceUsd;
            }

            var amountInCents = (long)(priceUsd * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "CoinsAmount", coinsAmount.ToString() },
                    { "CoinPackageId", coinPackageId.ToString() }
                }
            };

            var service = new PaymentIntentService(_stripeClient);
            var intent = await service.CreateAsync(options);

            return new PaymentIntentDto
            {
                ClientSecret = intent.ClientSecret,
                Amount = intent.Amount,
                Currency = intent.Currency
            };
        }

        public async Task ConfirmPurchaseAsync(string stripePaymentIntentId)
        {
            var existingTx = await _context.CoinTransactions
                .AnyAsync(t => t.StripePaymentIntentId == stripePaymentIntentId && t.Type == CoinTransactionType.Purchase);

            if (existingTx)
            {
                return; // Already processed
            }

            var service = new PaymentIntentService(_stripeClient);
            var intent = await service.GetAsync(stripePaymentIntentId);

            if (intent == null || intent.Status != "succeeded")
            {
                throw new BadRequestException("Payment intent did not succeed or not found");
            }

            if (!intent.Metadata.TryGetValue("UserId", out var userIdStr) ||
                !intent.Metadata.TryGetValue("CoinsAmount", out var coinsAmountStr) ||
                !int.TryParse(userIdStr, out var userId) ||
                !int.TryParse(coinsAmountStr, out var coinsAmount))
            {
                throw new BadRequestException("Invalid metadata in payment intent");
            }

            var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new UserWallet
                {
                    UserId = userId,
                    Balance = 0,
                    TotalEarned = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserWallets.Add(wallet);
            }

            wallet.Balance += coinsAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new CoinTransaction
            {
                UserId = userId,
                Type = CoinTransactionType.Purchase,
                Amount = coinsAmount,
                StripePaymentIntentId = stripePaymentIntentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SpendCoinsAsync(int userId, int chapterId)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null)
            {
                throw new NotFoundException("Chapter not found");
            }

            var authorId = chapter.Novel.UserId;

            // Check if already unlocked
            var alreadyUnlocked = await _context.UserChapterUnlocks
                .AnyAsync(u => u.UserId == userId && u.ChapterId == chapterId);

            if (alreadyUnlocked)
            {
                return true;
            }

            // Check pricing
            var pricing = await _context.ChapterPricings
                .FirstOrDefaultAsync(p => p.NovelId == chapter.NovelId);

            int price = 1; // Default price if no pricing settings found
            if (pricing != null)
            {
                if (chapter.ChapterNumber <= pricing.FreeChaptersCount)
                {
                    return true; // Free chapter
                }
                price = pricing.CoinPricePerChapter;
            }

            var readerWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (readerWallet == null || readerWallet.Balance < price)
            {
                throw new BadRequestException("Insufficient coin balance");
            }

            // Deduct from reader
            readerWallet.Balance -= price;
            readerWallet.UpdatedAt = DateTime.UtcNow;

            // Credit to author
            var authorWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == authorId);
            if (authorWallet == null)
            {
                authorWallet = new UserWallet
                {
                    UserId = authorId,
                    Balance = 0,
                    TotalEarned = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserWallets.Add(authorWallet);
            }
            authorWallet.Balance += price;
            authorWallet.TotalEarned += price;
            authorWallet.UpdatedAt = DateTime.UtcNow;

            // Create Unlock entry
            var unlock = new UserChapterUnlock
            {
                UserId = userId,
                ChapterId = chapterId,
                UnlockedAt = DateTime.UtcNow
            };
            _context.UserChapterUnlocks.Add(unlock);

            // Record Transactions
            var readerTx = new CoinTransaction
            {
                UserId = userId,
                Type = CoinTransactionType.ChapterUnlock,
                Amount = -price,
                RelatedChapterId = chapterId,
                CreatedAt = DateTime.UtcNow
            };

            var authorTx = new CoinTransaction
            {
                UserId = authorId,
                Type = CoinTransactionType.AuthorEarning,
                Amount = price,
                RelatedChapterId = chapterId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CoinTransactions.Add(readerTx);
            _context.CoinTransactions.Add(authorTx);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CoinTransactionDto>> GetTransactionsAsync(int userId)
        {
            var transactions = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CoinTransactionDto>>(transactions);
        }
    }
}
