using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel.Library;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class UserLibraryRepository : IUserLibraryRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private static readonly Dictionary<int, Dictionary<int, DateTime>> _lastUpdatedTimes = new();

        public UserLibraryRepository(BasicWebNovelContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        private DateTime GetLastUpdatedTime(int userId, int novelId)
        {
            if (_lastUpdatedTimes.TryGetValue(userId, out var userNovelTimes))
            {
                if (userNovelTimes.TryGetValue(novelId, out var lastUpdated))
                {
                    return lastUpdated;
                }
            }
            
            return default;
        }
        
        private void SetLastUpdatedTime(int userId, int novelId, DateTime dateTime)
        {
            if (!_lastUpdatedTimes.TryGetValue(userId, out var userNovelTimes))
            {
                userNovelTimes = new Dictionary<int, DateTime>();
                _lastUpdatedTimes[userId] = userNovelTimes;
            }
            
            userNovelTimes[novelId] = dateTime;
        }

        public async Task<List<GetUserLibraryDto>> GetUserLibraryAsync(int userId)
        {
            var userLibrary = await _context.UserLibraries
                .Include(ul => ul.Novel) 
                .Where(ul => ul.UserId == userId)
                .ToListAsync();
                
            // Create a sorted list of novels based on their last update time
            var sortedLibrary = new List<UserLibrary>();
            
            // First, try to get novels with known update times
            var novelIdsWithTimes = _lastUpdatedTimes.TryGetValue(userId, out var userNovelTimes)
                ? userNovelTimes.OrderByDescending(t => t.Value).Select(t => t.Key).ToList()
                : new List<int>();
                
            // Add novels with known times in order
            foreach (var novelId in novelIdsWithTimes)
            {
                var libraryEntry = userLibrary.FirstOrDefault(ul => ul.NovelId == novelId);
                if (libraryEntry != null)
                {
                    sortedLibrary.Add(libraryEntry);
                    userLibrary.Remove(libraryEntry);
                }
            }
            
            // Add remaining novels that don't have update times
            sortedLibrary.AddRange(userLibrary);
            
            var libraryDto = _mapper.Map<List<GetUserLibraryDto>>(sortedLibrary);
            return libraryDto;
        }

        public async Task<bool> IsNovelInUserLibraryAsync(int userId, int novelId)
        {
            return await _context.UserLibraries
                .AnyAsync(ul => ul.UserId == userId && ul.NovelId == novelId);
        }

        public async Task<bool> ResetAddedChapterAsync(int userId, int novelId)
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
            
            
            if (existingLibraryEntry == null)
            {
                throw new KeyNotFoundException("Library entry not found.");
            }

            
            existingLibraryEntry.AddedChapter = false;
            
            await _context.SaveChangesAsync();
            

            return true;
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
                
                // Also remove from in-memory tracking
                if (_lastUpdatedTimes.TryGetValue(userId, out var userNovelTimes))
                {
                    userNovelTimes.Remove(novelId);
                }
                
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
                
                // Track last updated time in memory
                SetLastUpdatedTime(userId, novelId, DateTime.UtcNow);
                
                return true;
            }
        }

        public async Task<bool> UpdateLastReadChapterAsync(int userId, int novelId, int lastReadChapter)
        {
            var userLibraryEntry = await _context.UserLibraries
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.NovelId == novelId);
                
            if (userLibraryEntry == null)
            {
                // If the novel is not in the library, add it
                return await AddNovelToUserLibraryAsync(userId, novelId, lastReadChapter);
            }
            
            // Update the existing entry
            userLibraryEntry.LastReadChapter = lastReadChapter;
            _context.UserLibraries.Update(userLibraryEntry);
            await _context.SaveChangesAsync();
            
            // Update the in-memory tracking
            SetLastUpdatedTime(userId, novelId, DateTime.UtcNow);
            
            return true;
        }
    }
}
