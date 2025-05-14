using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

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

        // S3 specific methods
        Task<Stream> GetUserImageAsync(int userId);
        Task<Stream> GetNovelImageAsync(int novelId);
        Task DeleteUserImageAsync(int userId);
        Task DeleteNovelImageAsync(int novelId);
    }
}
