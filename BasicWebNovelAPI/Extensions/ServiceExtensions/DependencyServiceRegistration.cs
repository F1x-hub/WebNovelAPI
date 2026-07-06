using BasicWebNovelAPI.Service.Abstractions;
using BasicWebNovelAPI.Service.BackgroundServices;
using BasicWebNovelAPI.Service.Implementations;
using Stripe;

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

            // Coins and Monetization
            services.AddScoped<ICoinService, CoinService>();
            services.AddScoped<IAuthorMonetizationService, AuthorMonetizationService>();
            services.AddScoped<IWithdrawalService, WithdrawalService>();
            services.AddScoped<IChapterAccessService, ChapterAccessService>();

            // Stripe Client
            services.AddSingleton<IStripeClient>(new StripeClient(configuration["Stripe:SecretKey"]));

            //background services
            services.AddHostedService<CodeExpireService>();
        }
    }
}
