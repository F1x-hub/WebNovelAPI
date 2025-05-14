using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel.Library;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class UserLibraryRepository : IUserLibraryRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;

        public UserLibraryRepository(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }


        public async Task<List<GetUserLibraryDto>> GetUserLibraryAsync(int userId)
        {
            var userLibrary = await _context.UserLibraries
                .Include(ul => ul.Novel) 
                .Where(ul => ul.UserId == userId)
                .ToListAsync();

            
            var libraryDto = _mapper.Map<List<GetUserLibraryDto>>(userLibrary);

            return libraryDto;
        }

        public async Task<bool> IsNovelInUserLibraryAsync(int userId, int novelId)
        {
            return await _context.UserLibraries
                .AnyAsync(ul => ul.UserId == userId && ul.NovelId == novelId);
        }

        public async Task<bool> AddNovelToUserLibraryAsync(int userId, int novelId, int lastReadChapter)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            
            var novel = await _context.Novels.FirstOrDefaultAsync(n => n.Id == novelId);
            if (novel == null)
            {
                throw new KeyNotFoundException("Novel not found.");
            }
            
            var existingLibraryEntry = await _context.UserLibraries
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.NovelId == novelId);

            if (existingLibraryEntry != null)
            {
                // Novel exists in library, so remove it
                _context.UserLibraries.Remove(existingLibraryEntry);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                // Novel doesn't exist in library, so add it
                var userLibrary = new UserLibrary
                {
                    UserId = userId,
                    NovelId = novelId,
                    LastReadChapter = lastReadChapter
                };

                await _context.UserLibraries.AddAsync(userLibrary);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
