using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel.Chapter;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Transactions;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public ChapterRepository(BasicWebNovelContext context, IMapper mapper, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<GetChapterDto> AddChapterToNovelAsync(int novelId, int userId, CreateChapterDto chapterDto)
        {
            var novel = await _context.Novels.FirstOrDefaultAsync(u => u.Id == novelId && u.UserId == userId);
            if (novel == null || novel.UserId != userId)
            {
                throw new KeyNotFoundException($"Novel with ID {novelId} not found.");
            }

            var chapter = _mapper.Map<Chapter>(chapterDto);
            chapter.NovelId = novelId;
            chapter.CreatedAt = DateTime.Now;
            
            // Check if we need to shift existing chapters
            var existingChapters = await _context.Chapters
                .Where(c => c.NovelId == novelId && c.ChapterNumber >= chapter.ChapterNumber)
                .OrderByDescending(c => c.ChapterNumber)
                .ToListAsync();

            // If chapter number is not specified or is 0, append as the last chapter
            if (chapter.ChapterNumber <= 0)
            {
                var lastChapter = await _context.Chapters
                    .Where(c => c.NovelId == novelId)
                    .OrderByDescending(c => c.ChapterNumber)
                    .FirstOrDefaultAsync();

                chapter.ChapterNumber = lastChapter?.ChapterNumber + 1 ?? 1;
            }
            else if (existingChapters.Any())
            {
                // Shift existing chapters to make room for the new one
                foreach (var existingChapter in existingChapters)
                {
                    existingChapter.ChapterNumber++;
                    _context.Chapters.Update(existingChapter);
                }
            }

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this novel's chapters
            await InvalidateNovelChaptersCache(novelId);
            
            var newChapterDto = _mapper.Map<GetChapterDto>(chapter);
            return newChapterDto;
        }

        public async Task<bool> UpdateChapterAsync(int novelId, int userId, int chapterId, UpdateChapterDto updateChapterDto)
        {
            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

            if (novel == null)
                return false;

            var existingChapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (existingChapter == null)
                return false;

            // Store the original chapter number and creation date before updating
            int originalChapterNumber = existingChapter.ChapterNumber;
            DateTime originalCreatedAt = existingChapter.CreatedAt;
            
            // Apply updates to the chapter
            _mapper.Map(updateChapterDto, existingChapter);
            
            // Preserve the original creation date
            existingChapter.CreatedAt = originalCreatedAt;
            
            // If chapter number changed, handle renumbering
            if (originalChapterNumber != existingChapter.ChapterNumber)
            {
                await ReorderChaptersAfterNumberChange(novelId, chapterId, originalChapterNumber, existingChapter.ChapterNumber);
            }
            
            _context.Chapters.Update(existingChapter);
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this novel's chapters
            await InvalidateNovelChaptersCache(novelId);
            
            return true;
        }

        public async Task<bool> DeleteChapterAsync(int novelId, int userId, int chapterId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var novel = await _context.Novels
                        .FirstOrDefaultAsync(n => n.Id == novelId && n.UserId == userId);

                    if (novel == null)
                        return false;

                    var chapter = await _context.Chapters
                        .Include(c => c.ChapterComments)
                        .Include(c => c.UserChapterRead)
                        .FirstOrDefaultAsync(c => c.Id == chapterId && c.NovelId == novelId);

                    if (chapter == null)
                        return false;

                    // Store chapter number before deletion
                    int deletedChapterNumber = chapter.ChapterNumber;

                    // Delete related entities manually to ensure proper cascading
                    // 1. Delete chapter comments
                    if (chapter.ChapterComments != null && chapter.ChapterComments.Any())
                    {
                        _context.RemoveRange(chapter.ChapterComments);
                    }

                    // 2. Delete user chapter read records
                    if (chapter.UserChapterRead != null && chapter.UserChapterRead.Any())
                    {
                        _context.RemoveRange(chapter.UserChapterRead);
                    }

                    // 3. Update UserLibrary entries that reference this chapter as last read
                    var userLibrariesToUpdate = await _context.UserLibraries
                        .Where(ul => ul.NovelId == novelId && ul.LastReadChapter == deletedChapterNumber)
                        .ToListAsync();

                    foreach (var userLibrary in userLibrariesToUpdate)
                    {
                        // Find the previous chapter number, if any
                        var previousChapter = await _context.Chapters
                            .Where(c => c.NovelId == novelId && c.ChapterNumber < deletedChapterNumber)
                            .OrderByDescending(c => c.ChapterNumber)
                            .FirstOrDefaultAsync();

                        userLibrary.LastReadChapter = previousChapter?.ChapterNumber ?? 0;
                        _context.Update(userLibrary);
                    }

                    // 4. Now delete the chapter itself
                    _context.Chapters.Remove(chapter);

                    // 5. Save all the changes we've made so far
                    await _context.SaveChangesAsync();

                    // 6. Reorder remaining chapters after deletion
                    var chaptersToUpdate = await _context.Chapters
                        .Where(c => c.NovelId == novelId && c.ChapterNumber > deletedChapterNumber)
                        .OrderBy(c => c.ChapterNumber)
                        .ToListAsync();
                    
                    foreach (var chapterToUpdate in chaptersToUpdate)
                    {
                        chapterToUpdate.ChapterNumber--;
                        _context.Chapters.Update(chapterToUpdate);
                    }

                    // 7. Save the chapter number updates
                    await _context.SaveChangesAsync();
                    
                    // 8. Commit transaction
                    await transaction.CommitAsync();
                    
                    // 9. Invalidate cache for this novel's chapters
                    await InvalidateNovelChaptersCache(novelId);
                    
                    return true;
                }
                catch (Exception)
                {
                    // In case of any error, rollback the transaction
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private async Task ReorderChaptersAfterNumberChange(int novelId, int chapterId, int oldNumber, int newNumber)
        {
            if (oldNumber == newNumber)
                return;
                
            var chapters = await _context.Chapters
                .Where(c => c.NovelId == novelId && c.Id != chapterId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();
                
            if (oldNumber < newNumber)
            {
                // Moving down - shift chapters between old and new positions up
                foreach (var chapter in chapters)
                {
                    if (chapter.ChapterNumber > oldNumber && chapter.ChapterNumber <= newNumber)
                    {
                        chapter.ChapterNumber--;
                        _context.Chapters.Update(chapter);
                    }
                }
            }
            else
            {
                // Moving up - shift chapters between new and old positions down
                foreach (var chapter in chapters)
                {
                    if (chapter.ChapterNumber >= newNumber && chapter.ChapterNumber < oldNumber)
                    {
                        chapter.ChapterNumber++;
                        _context.Chapters.Update(chapter);
                    }
                }
            }
        }
        
        private async Task InvalidateNovelChaptersCache(int novelId)
        {
            var cacheKey = $"chapters_{novelId}";
            await _cache.RemoveAsync(cacheKey);
            
            // Also invalidate individual chapter caches
            // We don't know which users might have viewed which chapters
            // so we can't precisely invalidate each chapter_novelId_chapterNumber_userId key
            // This is a limitation of the current caching approach
        }

        public async Task<List<GetChapterDto>> GetAllChaptersAsync(int novelId)
        {
            var cacheKey = $"chapters_{novelId}";
            var cachedChapters = await _cache.GetValue<List<GetChapterDto>>(cacheKey);
            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedChapters;
            }

            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                throw new Exception("Novel not found or access denied.");

            var readChapters = await _context.UserChapterReads
                                      .Where(ur => ur.Chapter.NovelId == novelId)
                                      .ToDictionaryAsync(ur => ur.ChapterId, ur => ur.IsRead);

            var chapterDtos = novel.Chapters
                                   .OrderBy(c => c.ChapterNumber)
                                   .Select(chapter =>
                                   {
                                       var dto = _mapper.Map<GetChapterDto>(chapter);
                                       dto.IsRead = readChapters.ContainsKey(chapter.Id) && readChapters[chapter.Id];
                                       return dto;
                                   })
                                   .ToList();

            await _cache.SetValue(cacheKey, chapterDtos);

            return chapterDtos;
        }

        public async Task<GetChapterDto?> GetChapterAsync(int novelId, int chapterNumber, int userId)
        {
            var cacheKey = $"chapter_{novelId}_{chapterNumber}_{userId}";
            var cachedChapter = await _cache.GetValue<GetChapterDto>(cacheKey);
            if (!string.IsNullOrEmpty(_cache.GetString(cacheKey)))
            {
                return cachedChapter;
            }

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);

            if (chapter == null)
            {
                return null;
            }

            var chapterDto = _mapper.Map<GetChapterDto>(chapter);
            
            // Only process read status if user is authenticated
            if (userId > 0)
            {
                var userChapterRead = await _context.UserChapterReads
                    .FirstOrDefaultAsync(urc => urc.UserId == userId && urc.ChapterId == chapter.Id);

                if (userChapterRead == null)
                {
                    userChapterRead = new UserChapterRead()
                    {
                        UserId = userId,
                        ChapterId = chapter.Id,
                        IsRead = true
                    };

                    _context.UserChapterReads.Add(userChapterRead);
                }
                else if (!userChapterRead.IsRead)
                {
                    userChapterRead.IsRead = true;
                    _context.UserChapterReads.Update(userChapterRead);
                }

                chapterDto.IsRead = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                // For unauthenticated users, chapter is not marked as read
                chapterDto.IsRead = false;
            }

            await _cache.SetValue(cacheKey, chapterDto);

            return chapterDto;
        }

        public async Task<bool> UpdateLastReadChapterAsync(int userId, int novelId, int chapterNumber)
        {
            if (userId <= 0)
            {
                return false;
            }
            
            // Verify the novel exists
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
            {
                return false;
            }

            // Special case: if chapterNumber is 0, it means we're clearing the last read chapter
            // No need to verify chapter existence in this case
            if (chapterNumber > 0)
            {
                // Only verify chapter exists if we're setting a specific chapter
                var chapter = await _context.Chapters
                    .FirstOrDefaultAsync(c => c.NovelId == novelId && c.ChapterNumber == chapterNumber);
                if (chapter == null)
                {
                    return false;
                }
            }

            // Update user library with last read chapter
            var userLibraryEntry = await _context.UserLibraries
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.NovelId == novelId);

            if (userLibraryEntry != null)
            {
                userLibraryEntry.LastReadChapter = chapterNumber;
                _context.UserLibraries.Update(userLibraryEntry);
            }
            else
            {
                // Create a new library entry if one doesn't exist
                userLibraryEntry = new UserLibrary
                {
                    UserId = userId,
                    NovelId = novelId,
                    LastReadChapter = chapterNumber
                };
                await _context.UserLibraries.AddAsync(userLibraryEntry);
            }

            await _context.SaveChangesAsync();
            
            // Invalidate relevant caches
            var userLibraryCacheKey = $"user_library_{userId}";
            await _cache.RemoveAsync(userLibraryCacheKey);
            
            return true;
        }

        public async Task<int> GetLastReadChapterAsync(int userId, int novelId)
        {
            if (userId <= 0)
            {
                return 0;
            }
            
            // Check if the novel exists
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
            {
                return 0;
            }
            
            // Get the user's library entry for this novel
            var userLibraryEntry = await _context.UserLibraries
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.NovelId == novelId);
            
            // Return the last read chapter or 0 if not found
            return userLibraryEntry?.LastReadChapter ?? 0;
        }
    }
}
