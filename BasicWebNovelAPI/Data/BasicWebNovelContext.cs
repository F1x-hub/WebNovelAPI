using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using BasicWebNovelAPI.Model.Coins;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Data
{
    public class BasicWebNovelContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserImages> UserImages { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Novel> Novels { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<NovelImages> NovelImages { get; set; } = null!;
        public DbSet<NovelGenre> NovelGenres { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<UserLibrary> UserLibraries { get; set; } = null!;
        public DbSet<NovelComments> NovelComments { get; set; } = null!;
        public DbSet<ChapterComments> ChapterComments { get; set; } = null!;
        public DbSet<UserChapterRead> UserChapterReads { get; set; } = null!;
        public DbSet<NovelView> NovelViews { get; set; } = null!;
        
        public DbSet<CoinPackage> CoinPackages { get; set; } = null!;
        public DbSet<UserWallet> UserWallets { get; set; } = null!;
        public DbSet<CoinTransaction> CoinTransactions { get; set; } = null!;
        public DbSet<ChapterPricing> ChapterPricings { get; set; } = null!;
        public DbSet<UserChapterUnlock> UserChapterUnlocks { get; set; } = null!;
        public DbSet<AuthorWithdrawal> AuthorWithdrawals { get; set; } = null!;
        
        public DbSet<NovelCommentLikes> NovelCommentLikes { get; set; } = null!;
        public DbSet<ChapterCommentLikes> ChapterCommentLikes { get; set; } = null!;
        
        

        public BasicWebNovelContext(DbContextOptions options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NovelView>()
                .HasOne(nv => nv.Novel)
                .WithMany(n => n.NovelViews)
                .HasForeignKey(nv => nv.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NovelView>()
                .HasOne(nv => nv.User)
                .WithMany()
                .HasForeignKey(nv => nv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserImages)
                .WithOne(ui => ui.User)
                .HasForeignKey(ui => ui.UserId);

            
            modelBuilder.Entity<Novel>()
                .HasOne(w => w.User)
                .WithMany(u => u.Novels)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Novel)
                .WithMany(w => w.Ratings)
                .HasForeignKey(r => r.NovelId)
                .OnDelete(DeleteBehavior.Restrict); 

            
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<NovelGenre>()
                .HasKey(wg => new { wg.NovelId, wg.GenreId });

            modelBuilder.Entity<NovelGenre>()
                .HasOne(wg => wg.Novel)
                .WithMany(w => w.NovelGenres)
                .HasForeignKey(wg => wg.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NovelGenre>()
                .HasOne(wg => wg.Genre)
                .WithMany(g => g.NovelGenres)
                .HasForeignKey(wg => wg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<UserLibrary>()
                .HasOne(ul => ul.Novel)
                .WithMany()
                .HasForeignKey(ul => ul.NovelId)
                .OnDelete(DeleteBehavior.Restrict); 

            
            modelBuilder.Entity<UserLibrary>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.Library)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "User" },
                new Role { Id = 2, RoleName = "Admin" }
            );

            modelBuilder.Entity<Novel>()
                .HasMany(w => w.Chapters)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.NovelComments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            
            modelBuilder.Entity<Novel>()
                .HasMany(n => n.NovelComments)
                .WithOne(c => c.Novel)
                .HasForeignKey(c => c.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChapterComments>()
                .HasOne(cc => cc.User)
                .WithMany(u => u.ChapterComments)
                .HasForeignKey(cc => cc.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            

            modelBuilder.Entity<ChapterComments>()
                .HasOne(cc => cc.Chapter)
                .WithMany(c => c.ChapterComments)
                .HasForeignKey(cc => cc.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chapter>()
             .HasOne(c => c.Novel)
             .WithMany(n => n.Chapters)
             .HasForeignKey(c => c.NovelId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChapterRead>()
                .HasOne(ucr => ucr.User)
                .WithMany(u => u.UserChapterRead)
                .HasForeignKey(ucr => ucr.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Use Restrict

            modelBuilder.Entity<UserChapterRead>()
                .HasOne(ucr => ucr.Chapter)
                .WithMany(c => c.UserChapterRead)
                .HasForeignKey(ucr => ucr.ChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChapterCommentLikes>()
                .HasOne(ccl => ccl.User)
                .WithMany()
                .HasForeignKey(ccl => ccl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChapterCommentLikes>()
                .HasOne(ccl => ccl.ChapterComment)
                .WithMany(cc => cc.Likes)
                .HasForeignKey(ccl => ccl.ChapterCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NovelCommentLikes>()
                .HasOne(ccl => ccl.User)
                .WithMany()
                .HasForeignKey(ccl => ccl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NovelCommentLikes>()
                .HasOne(ccl => ccl.NovelComment)
                .WithMany(cc => cc.Likes)
                .HasForeignKey(ccl => ccl.NovelCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Fantasy" },
                new Genre { Id = 2, Name = "Science Fiction" },
                new Genre { Id = 3, Name = "Romance" },
                new Genre { Id = 4, Name = "Action" },
                new Genre { Id = 5, Name = "Horror" },
                new Genre { Id = 6, Name = "Mystery" }
            );

            // Coin system entity configurations
            modelBuilder.Entity<UserWallet>()
                .HasOne(uw => uw.User)
                .WithMany()
                .HasForeignKey(uw => uw.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoinTransaction>()
                .HasOne(ct => ct.User)
                .WithMany()
                .HasForeignKey(ct => ct.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoinTransaction>()
                .HasOne(ct => ct.RelatedChapter)
                .WithMany()
                .HasForeignKey(ct => ct.RelatedChapterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChapterPricing>()
                .HasOne(cp => cp.Novel)
                .WithMany()
                .HasForeignKey(cp => cp.NovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChapterUnlock>()
                .HasOne(ucu => ucu.User)
                .WithMany()
                .HasForeignKey(ucu => ucu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserChapterUnlock>()
                .HasOne(ucu => ucu.Chapter)
                .WithMany()
                .HasForeignKey(ucu => ucu.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuthorWithdrawal>()
                .HasOne(aw => aw.Author)
                .WithMany()
                .HasForeignKey(aw => aw.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoinPackage>()
                .Property(p => p.PriceUsd)
                .HasColumnType("decimal(18,2)");

            // Seed coin packages
            modelBuilder.Entity<CoinPackage>().HasData(
                new CoinPackage { Id = 1, CoinsAmount = 100, PriceUsd = 1.00m, IsCustom = false, IsActive = true },
                new CoinPackage { Id = 2, CoinsAmount = 200, PriceUsd = 2.00m, IsCustom = false, IsActive = true },
                new CoinPackage { Id = 3, CoinsAmount = 500, PriceUsd = 5.00m, IsCustom = false, IsActive = true },
                new CoinPackage { Id = 4, CoinsAmount = 1000, PriceUsd = 10.00m, IsCustom = false, IsActive = true },
                new CoinPackage { Id = 5, CoinsAmount = 0, PriceUsd = 0.00m, IsCustom = true, IsActive = true }
            );
        }

    }
}
