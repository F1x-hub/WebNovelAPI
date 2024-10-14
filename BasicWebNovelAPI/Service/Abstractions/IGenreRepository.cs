using BasicWebNovelAPI.Model.Dto.Novel;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IGenreRepository
    {
        Task<GetGenreDto> CreateGenreAsync(CreateGenreDto createGenreDto);
    }
}
