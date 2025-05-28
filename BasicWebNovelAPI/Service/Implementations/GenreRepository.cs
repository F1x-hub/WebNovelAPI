using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel.Genre;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class GenreRepository : IGenreRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IMapper _mapper;
        public GenreRepository(BasicWebNovelContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GetGenreDto> CreateGenreAsync(CreateGenreDto createGenreDto)
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

        public async Task<List<GetGenreDto>> GetGenresAsync() 
        {
            var genre = await _context.Genres
                .Include(g => g.NovelGenres)
                .ToListAsync();

            var genreDto = _mapper.Map<List<GetGenreDto>>(genre);

            return genreDto;
        }

        public async Task<GetGenreDto> UpdateGenreAsync(int genreId, UpdateGenreDto updateGenreDto)
        {
            var genre = await _context.Genres.FindAsync(genreId);
            if (genre == null)
            {
                throw new KeyNotFoundException($"Genre with ID {genreId} not found");
            }
            
            // Check if name is being changed to an existing name
            if (!string.Equals(genre.Name, updateGenreDto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingGenre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.Name == updateGenreDto.Name);
                
                if (existingGenre != null)
                {
                    throw new Exception("A genre with this name already exists");
                }
            }
            
            // Update genre properties
            genre.Name = updateGenreDto.Name;
            
            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();
            
            return _mapper.Map<GetGenreDto>(genre);
        }

        public async Task<bool> DeleteGenreAsync(int genreId)
        {
            var genre = await _context.Genres
                .Include(g => g.NovelGenres)
                .FirstOrDefaultAsync(g => g.Id == genreId);
                
            if (genre == null)
            {
                return false;
            }
            
            // Check if this genre is associated with any novels
            if (genre.NovelGenres != null && genre.NovelGenres.Any())
            {
                throw new Exception("Cannot delete genre because it is used by one or more novels");
            }
            
            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}
