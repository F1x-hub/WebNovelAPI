using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;

namespace BasicWebNovelAPI.Service.Abstractions
{
    public interface IImageRepository
    {
        Task<string> GenerateNovelImageSource(IFormFile imageFile);

        Task<string> GenerateUserImageSource(IFormFile imageFile);

        Task SaveNovelImageInDatabase(NovelImages novelImages);

        Task SaveUserImageInDatabase(UserImages userImage);


        Task AddNovelImagesAsync(int novelId, IFormFile? imageFiles);

        Task AddUserImagesAsync(int userId, IFormFile? imageFiles);


    }
}
