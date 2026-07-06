using System;
using System.Threading.Tasks;
using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Exceptions;
using BasicWebNovelAPI.Model.Coins;
using BasicWebNovelAPI.Model.Dto.Coins;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class AuthorMonetizationService : IAuthorMonetizationService
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public AuthorMonetizationService(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ChapterPricingDto> GetPricingAsync(int novelId)
        {
            var novelExists = await _context.Novels.AnyAsync(n => n.Id == novelId);
            if (!novelExists)
            {
                throw new NotFoundException("Novel not found");
            }

            var pricing = await _context.ChapterPricings
                .FirstOrDefaultAsync(p => p.NovelId == novelId);

            if (pricing == null)
            {
                // Return default pricing values
                return new ChapterPricingDto
                {
                    NovelId = novelId,
                    FreeChaptersCount = 10,
                    CoinPricePerChapter = 1,
                    UnlockScheduleEnabled = false,
                    UnlockIntervalDays = 7,
                    ScheduleStartDate = null
                };
            }

            return _mapper.Map<ChapterPricingDto>(pricing);
        }

        public async Task<ChapterPricingDto> SavePricingAsync(int authorId, int novelId, UpdatePricingRequest request)
        {
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
            {
                throw new NotFoundException("Novel not found");
            }

            if (novel.UserId != authorId)
            {
                throw new BadRequestException("You are not authorized to manage monetization for this novel");
            }

            var pricing = await _context.ChapterPricings
                .FirstOrDefaultAsync(p => p.NovelId == novelId);

            if (pricing == null)
            {
                pricing = new ChapterPricing
                {
                    NovelId = novelId
                };
                _context.ChapterPricings.Add(pricing);
            }

            _mapper.Map(request, pricing);
            pricing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<ChapterPricingDto>(pricing);
        }
    }
}
