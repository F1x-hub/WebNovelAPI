using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Dto.Novel;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class NovelRepository : INovelRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IImageRepository _imageRepository;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        

        public NovelRepository(BasicWebNovelContext context, IMapper mapper, IImageRepository imageRepository, IDistributedCache cache)
        {
            _context = context;
            _mapper = mapper;
            _imageRepository = imageRepository;
            _cache = cache;   
        }

        
        public async Task<List<GetNovelDto>> GetNovels()
        {
            /*var CacheKey = "novels";
            var cachedNovels = await _cache.GetValue<List<GetNovelDto>>(CacheKey);
            if (cachedNovels != null)
            {
                return cachedNovels;
            }*/

            var novels = await _context.Novels
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            //await _cache.SetValue(CacheKey, novelDtos, TimeSpan.FromMinutes(10));

            return novelDtos;
        }

        
        public async Task<GetNovelDto> GetNovelById(int novelId)
        {
            
            var novel = await _context.Novels
                .Include(n => n.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                return null;

            var novelDtos = _mapper.Map<GetNovelDto>(novel);

            return novelDtos;
        }

        public async Task<List<GetNovelDto>> GetNovelByName(string title)
        {
            var novels = await _context.Novels
                .Where(n => n.Title.ToLower().Contains(title.ToLower()))
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            return novelDtos;
        }


        public async Task<List<GetNovelDto>> GetUserAllNovel(int userId)
        {
            var novels = await _context.Novels
                .Where(n => n.UserId == userId)
                .Include(n => n.Chapters)
                .ToListAsync();

            var novelDtos = _mapper.Map<List<GetNovelDto>>(novels);

            return novelDtos;

        }

        
        public async Task<GetNovelDto> CreateNovel(CreateNovelDto createNovelDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == createNovelDto.UserId);

            if (user == null)
                throw new Exception("User Not Found");

            var novel = _mapper.Map<Novel>(createNovelDto);
            novel.UserId = createNovelDto.UserId; 
            novel.PublishedDate = DateTime.Now;

            if (createNovelDto.GenreIds != null && createNovelDto.GenreIds.Count > 0)
            {
                novel.NovelGenres = new List<NovelGenre>();

                foreach (var genreId in createNovelDto.GenreIds)
                {
                    var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Id == genreId);
                    if (genre != null)
                    {
                        novel.NovelGenres.Add(new NovelGenre
                        {
                            GenreId = genre.Id,
                            Novel = novel
                        });
                    }
                }
            }


            await _context.Novels.AddAsync(novel);
            await _context.SaveChangesAsync();

            var novelDtos = _mapper.Map<GetNovelDto>(novel);

            return novelDtos;

        }

        public async Task AddNovelImagesAsync(int novelId, IFormFile? imageFiles)
        {
            var novel = await _context.Novels.FirstOrDefaultAsync(u => u.Id == novelId);

            if (novel == null)
            {
                throw new Exception("Novel Not Found");
            }

            if (imageFiles != null)
            {
                var userImage = new NovelImages
                {
                    NovelId = novel.Id,
                    ImageSource = await _imageRepository.GenerateNovelImageSource(imageFiles)
                };
                await _imageRepository.SaveNovelImageInDatabase(userImage);

            }


        }

        public async Task<GetGenreDto> CreateGenre(CreateGenreDto createGenreDto)
        {
            
            var existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == createGenreDto.Name);
            if (existingGenre != null)
            {
                throw new Exception("Genre already exists");
            }

            
            var genre = _mapper.Map<Genre>(createGenreDto);

            
            await _context.Genres.AddAsync(genre);
            await _context.SaveChangesAsync();

            var genreDtos = _mapper.Map<GetGenreDto>(genre);

            return genreDtos; 
        }



        public async Task<bool> UpdateNovel(int id, UpdateNovelDto updateNovelDto)
        {
            var existingNovel = await _context.Novels
                .Include (n => n.Chapters)
                .FirstOrDefaultAsync (n => n.Id == id);

            if(existingNovel == null)
                return false;

            _mapper.Map(updateNovelDto, existingNovel);

            _context.Novels.Update(existingNovel);
            await _context.SaveChangesAsync();

            return true;

        }

        
        public async Task<bool> DeleteNovel(int novelId)
        {
            var novel = await _context.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if(novel == null) 
                return false;

            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();

            return true;

        }

        // Add a new chapter to a novel
        public async Task<Chapter> AddChapterToNovelAsync(int novelId, CreateChapterDto chapterDto)
        {
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel == null)
            {
                throw new KeyNotFoundException($"Novel with ID {novelId} not found.");
            }

            
            var lastChapter = await _context.Chapters
                .Where(c => c.NovelId == novelId)
                .OrderByDescending(c => c.ChapterNumber)
                .FirstOrDefaultAsync();

            
            int nextChapterNumber = lastChapter?.ChapterNumber + 1 ?? 1;

            
            var chapter = _mapper.Map<Chapter>(chapterDto);
            chapter.NovelId = novelId;
            chapter.ChapterNumber = nextChapterNumber;

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            return chapter;
        }

        // Update an existing chapter in a novel
        public async Task<bool> UpdateChapter(int novelId, int chapterId, Chapter updatedChapter)
        {
            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                throw new Exception("Novel not found");

            var chapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                throw new Exception("Chapter not found");

            chapter.Title = updatedChapter.Title;
            chapter.Content = updatedChapter.Content;
            chapter.ChapterNumber = updatedChapter.ChapterNumber;

            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete a chapter from a novel
        public async Task<bool> DeleteChapter(int novelId, int chapterId)
        {
            var novel = await _context.Novels.Include(n => n.Chapters)
                                             .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel == null)
                throw new Exception("Novel not found");

            var chapter = novel.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
                return false;

            novel.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
