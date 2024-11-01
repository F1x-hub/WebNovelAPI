using BasicWebNovelAPI.Model.Novels;
using BasicWebNovelAPI.Model.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace BasicWebNovelAPI.Data
{
    public class BasicWebNovelContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserImages> UserImages { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Novel> Novels { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<NovelImages> NovelImages { get; set; }
        public DbSet<NovelGenre> NovelGenres { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<UserLibrary> UserLibraries { get; set; }
        public DbSet<NovelComments> NovelComments { get; set; }
        public DbSet<ChapterComments> ChapterComments { get; set; }


        public BasicWebNovelContext(DbContextOptions options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


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
        }

    }
}
