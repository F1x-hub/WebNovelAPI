using BasicWebNovelAPI.Model.Dto.Novel.Genre;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IGenreRepository
    {
        Task<GetGenreDto> CreateGenreAsync(CreateGenreDto createGenreDto);
        Task<List<GetGenreDto>> GetGenresAsync();
    }
}
