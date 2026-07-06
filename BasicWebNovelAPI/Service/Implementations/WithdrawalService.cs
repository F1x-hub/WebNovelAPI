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

namespace BasicWebNovelAPI.Service.Implementations
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public WithdrawalService(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<WithdrawalDto> RequestWithdrawalAsync(int authorId, int coinsAmount)
        {
            if (coinsAmount <= 0)
            {
                throw new BadRequestException("Withdrawal amount must be greater than zero");
            }

            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == authorId);

            if (wallet == null || wallet.Balance < coinsAmount)
            {
                throw new BadRequestException("Insufficient balance for withdrawal");
            }

            var platformFee = (int)Math.Floor(coinsAmount * 0.20);
            var netCoins = coinsAmount - platformFee;

            wallet.Balance -= coinsAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var withdrawal = new AuthorWithdrawal
            {
                AuthorId = authorId,
                CoinsAmount = coinsAmount,
                PlatformFeeCoins = platformFee,
                NetCoins = netCoins,
                Status = WithdrawalStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            var transaction = new CoinTransaction
            {
                UserId = authorId,
                Type = CoinTransactionType.Withdrawal,
                Amount = -coinsAmount,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuthorWithdrawals.Add(withdrawal);
            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return _mapper.Map<WithdrawalDto>(withdrawal);
        }

        public async Task<IEnumerable<WithdrawalDto>> GetWithdrawalsAsync(int authorId)
        {
            var withdrawals = await _context.AuthorWithdrawals
                .Where(w => w.AuthorId == authorId)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WithdrawalDto>>(withdrawals);
        }
    }
}
