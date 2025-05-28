using BasicWebNovelAPI.Model.Dto.Novel.Genre;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IGenreRepository
    {
        Task<GetGenreDto> CreateGenreAsync(CreateGenreDto createGenreDto);
        Task<List<GetGenreDto>> GetGenresAsync();
        Task<GetGenreDto> UpdateGenreAsync(int genreId, UpdateGenreDto updateGenreDto);
        Task<bool> DeleteGenreAsync(int genreId);
    }
}
