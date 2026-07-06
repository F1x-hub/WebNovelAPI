using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ImageRepository : IImageRepository
    {
        private readonly BasicWebNovelContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _userImagesFolder;
        private readonly string _novelImagesFolder;

        public ImageRepository(BasicWebNovelContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
            _userImagesFolder = Path.Combine(uploadsPath, "user-images");
            _novelImagesFolder = Path.Combine(uploadsPath, "novel-images");

            if (!Directory.Exists(_userImagesFolder)) Directory.CreateDirectory(_userImagesFolder);
            if (!Directory.Exists(_novelImagesFolder)) Directory.CreateDirectory(_novelImagesFolder);
        }

        public async Task AddNovelImagesAsync(int novelId, IFormFile? imageFile)
        {
            var novel = await _context.Novels.FirstOrDefaultAsync(u => u.Id == novelId);

            if (novel == null)
            {
                throw new Exception("Novel Not Found");
            }

            if (imageFile != null)
            {
                var existingImage = await _context.NovelImages.FirstOrDefaultAsync(i => i.NovelId == novelId);
                
                if (existingImage != null)
                {
                    // Delete existing local file if exists
                    if (!string.IsNullOrWhiteSpace(existingImage.ImageSource) && !existingImage.ImageSource.StartsWith("http"))
                    {
                        var fullPath = Path.Combine(_environment.ContentRootPath, existingImage.ImageSource.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }
                    
                    // Update the existing record
                    existingImage.ImageSource = await GenerateNovelImageSource(imageFile);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var novelImage = new NovelImages
                    {
                        NovelId = novel.Id,
                        ImageSource = await GenerateNovelImageSource(imageFile)
                    };
                    await SaveNovelImageInDatabase(novelImage);
                }
            }
        }

        public async Task AddUserImagesAsync(int userId, IFormFile? imageFile)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new Exception("User Not Found");
            }

            if (imageFile != null)
            {
                var existingImage = await _context.UserImages.FirstOrDefaultAsync(i => i.UserId == userId);
                
                if (existingImage != null)
                {
                    // Delete existing local file if exists
                    if (!string.IsNullOrWhiteSpace(existingImage.ImageSource) && !existingImage.ImageSource.StartsWith("http"))
                    {
                        var fullPath = Path.Combine(_environment.ContentRootPath, existingImage.ImageSource.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }
                    
                    // Update the existing record
                    existingImage.ImageSource = await GenerateUserImageSource(imageFile);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var userImage = new UserImages
                    {
                        UserId = user.Id,
                        ImageSource = await GenerateUserImageSource(imageFile)
                    };
                    await SaveUserImageInDatabase(userImage);
                }
            }
        }

        public async Task<string> GenerateNovelImageSource(IFormFile imageFile)
        {
            var fileName = GenerateUniqueFileName(imageFile.FileName);
            var filePath = Path.Combine(_novelImagesFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            return $"uploads/novel-images/{fileName}";
        }

        public async Task<string> GenerateUserImageSource(IFormFile imageFile)
        {
            var fileName = GenerateUniqueFileName(imageFile.FileName);
            var filePath = Path.Combine(_userImagesFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            return $"uploads/user-images/{fileName}";
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

        public async Task<Stream> GetNovelImageAsync(int novelId)
        {
            var novel = await _context.NovelImages.FirstOrDefaultAsync(n => n.NovelId == novelId);
            
            if (novel == null || string.IsNullOrWhiteSpace(novel.ImageSource))
            {
                throw new Exceptions.NotFoundException($"Image for novel with id {novelId} not found");
            }
            
            // Handle legacy S3 URLs or local paths
            if (novel.ImageSource.StartsWith("http"))
            {
                // If it's a legacy S3 URL and user has no access, we should probably return a default image
                // or try to download it once. For now, since user has NO access, let's just throw or return default.
                throw new Exceptions.NotFoundException("Legacy AWS S3 image no longer accessible");
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, novel.ImageSource.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new Exceptions.NotFoundException("Local image file not found");
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }

        public async Task<Stream> GetUserImageAsync(int userId)
        {
            var user = await _context.UserImages.FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null || string.IsNullOrWhiteSpace(user.ImageSource))
            {
                throw new Exceptions.NotFoundException($"Image for user with id {userId} not found");
            }
            
            if (user.ImageSource.StartsWith("http"))
            {
                throw new Exceptions.NotFoundException("Legacy AWS S3 image no longer accessible");
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, user.ImageSource.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new Exceptions.NotFoundException("Local image file not found");
            }

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }

        public async Task DeleteNovelImageAsync(int novelId)
        {
            var novel = await _context.NovelImages.FirstOrDefaultAsync(n => n.NovelId == novelId);
            
            if (novel == null)
            {
                throw new Exception($"Image for novel with id {novelId} not found");
            }
            
            if (!string.IsNullOrWhiteSpace(novel.ImageSource) && !novel.ImageSource.StartsWith("http"))
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, novel.ImageSource.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            
            _context.NovelImages.Remove(novel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserImageAsync(int userId)
        {
            var user = await _context.UserImages.FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null)
            {
                throw new Exception($"Image for user with id {userId} not found");
            }
            
            if (!string.IsNullOrWhiteSpace(user.ImageSource) && !user.ImageSource.StartsWith("http"))
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, user.ImageSource.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            
            _context.UserImages.Remove(user);
            await _context.SaveChangesAsync();
        }
        
        private string GenerateUniqueFileName(string originalFileName)
        {
            return $"{Guid.NewGuid()}_{originalFileName}";
        }
    }
}
