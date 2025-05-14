using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BasicWebNovelAPI.Data;
using BasicWebNovelAPI.Extensions;
using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Service.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BasicWebNovelAPI.Service.Implementations
{
    public class ImageRepository : IImageRepository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly BasicWebNovelContext _context;
        private readonly string _imageBucketName;
        private readonly string _novelBucketName;

        public ImageRepository(IAmazonS3 s3Client, BasicWebNovelContext context, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _context = context;
            _imageBucketName = configuration["AWS:ImageBucketName"];
            _novelBucketName = configuration["AWS:NovelBucketName"];
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
                    // Delete existing image from S3 if exists
                    if (!string.IsNullOrWhiteSpace(existingImage.ImageSource))
                    {
                        var uri = new Uri(existingImage.ImageSource);
                        var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
                        await _s3Client.DeleteObjectAsync(_novelBucketName, key);
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
                    // Delete existing image from S3 if exists
                    if (!string.IsNullOrWhiteSpace(existingImage.ImageSource))
                    {
                        var uri = new Uri(existingImage.ImageSource);
                        var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
                        await _s3Client.DeleteObjectAsync(_imageBucketName, key);
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
            
            using (var stream = imageFile.OpenReadStream())
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(stream, _novelBucketName, fileName);
            }
            
            return $"https://{_novelBucketName}.s3.amazonaws.com/{fileName}";
        }

        public async Task<string> GenerateUserImageSource(IFormFile imageFile)
        {
            var fileName = GenerateUniqueFileName(imageFile.FileName);
            
            using (var stream = imageFile.OpenReadStream())
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(stream, _imageBucketName, fileName);
            }
            
            return $"https://{_imageBucketName}.s3.amazonaws.com/{fileName}";
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
            
            var uri = new Uri(novel.ImageSource);
            var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            
            var response = await _s3Client.GetObjectAsync(_novelBucketName, key);
            return response.ResponseStream;
        }

        public async Task<Stream> GetUserImageAsync(int userId)
        {
            var user = await _context.UserImages.FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null || string.IsNullOrWhiteSpace(user.ImageSource))
            {
                throw new Exceptions.NotFoundException($"Image for user with id {userId} not found");
            }
            
            var uri = new Uri(user.ImageSource);
            var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            
            var response = await _s3Client.GetObjectAsync(_imageBucketName, key);
            return response.ResponseStream;
        }

        public async Task DeleteNovelImageAsync(int novelId)
        {
            var novel = await _context.NovelImages.FirstOrDefaultAsync(n => n.NovelId == novelId);
            
            if (novel == null)
            {
                throw new Exception($"Image for novel with id {novelId} not found");
            }
            
            if (string.IsNullOrWhiteSpace(novel.ImageSource))
            {
                return;
            }
            
            var uri = new Uri(novel.ImageSource);
            var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            
            await _s3Client.DeleteObjectAsync(_novelBucketName, key);
            
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
            
            if (string.IsNullOrWhiteSpace(user.ImageSource))
            {
                return;
            }
            
            var uri = new Uri(user.ImageSource);
            var key = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            
            await _s3Client.DeleteObjectAsync(_imageBucketName, key);
            
            _context.UserImages.Remove(user);
            await _context.SaveChangesAsync();
        }
        
        private string GenerateUniqueFileName(string originalFileName)
        {
            return $"{Guid.NewGuid()}_{originalFileName}";
        }
    }
}
