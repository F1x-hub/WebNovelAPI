using AutoMapper;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Model.Dto.Novel;
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
    }
}
