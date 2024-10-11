using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _environment;
        private readonly BasicWebNovelContext _context;
        public ImageRepository(IWebHostEnvironment environment, BasicWebNovelContext context) 
        {
            _context = context;
            _environment = environment;
        }

        public async Task<string> GenerateNovelImageSource(IFormFile imageFile)
        {
            string contentPath = _environment.ContentRootPath;
            string folder = Descriptive.CreateImageDirectory(contentPath, Path.Combine("Uploads", "NovelImages\\"));

            string newFileName = Descriptive.GenerateImageSourceWithExtention(folder, imageFile);

            await Descriptive.WriteImageInFileAsync(newFileName, imageFile);

            return newFileName;
        }

        public async Task<string> GenerateUserImageSource(IFormFile imageFile)
        {
            string contentPath = _environment.ContentRootPath;
            string folder = Descriptive.CreateImageDirectory(contentPath, Path.Combine("Uploads", "UserImages\\"));

            string newFileName = Descriptive.GenerateImageSourceWithExtention(folder, imageFile);

            await Descriptive.WriteImageInFileAsync(newFileName, imageFile);

            return newFileName;
        }

        public async Task SaveNovelImageInDatabase(NovelImages novelImage)
        {
            await _context.NovelImages.AddAsync(novelImage);
            await _context.SaveChangesAsync();
        }

        public async Task SaveUserImageInDatabase(UserImages userImage)
        {
            await _context.UserImages.AddAsync(userImage);
            await _context.SaveChangesAsync();
        }
    }
}
