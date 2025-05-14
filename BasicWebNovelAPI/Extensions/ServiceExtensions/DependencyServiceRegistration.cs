using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.BackgroundServices;
using BasicWebNovelAPI.Service.Implementations;

namespace BasicWebNovelAPI.Extensions.ServiceExtensions
{
    public static class DependencyServiceRegistration
    {
        public static void AddDependencyServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<INovelRepository, NovelRepository>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<IUserLibraryRepository, UserLibraryRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            // Configure AWS S3 Client
            services.AddAWSService<IAmazonS3>(new AWSOptions
            {
                Credentials = new Amazon.Runtime.BasicAWSCredentials(
                    configuration["AWS:AccessKey"],
                    configuration["AWS:SecretKey"]),
                Region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
            });

            //background services
            services.AddHostedService<CodeExpireService>();
        }
    }
}
