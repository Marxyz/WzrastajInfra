using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ArticleMetadataEntity> Articles { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>(ConfigureUser);
        }

        public void ConfigureUser(EntityTypeBuilder<UserEntity> builder)
        {
            var navigationRefresh = builder.Metadata.FindNavigation(nameof(UserEntity.RefreshTokens));
            //EF access the RefreshTokens collection property through its backing field
            navigationRefresh.SetPropertyAccessMode(PropertyAccessMode.Field);
            builder.Ignore(b => b.PasswordHash);
            builder.Ignore(b => b.Email);
            builder.Ignore(b => b.DisplayName);
        }

        public override int SaveChanges()
        {
            AddAuitInfo();
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            AddAuitInfo();
            return await base.SaveChangesAsync();
        }

        private void AddAuitInfo()
        {
            var entries = ChangeTracker.Entries().Where(x =>
                x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((BaseEntity)entry.Entity).Created = DateTime.UtcNow;
                }

                ((BaseEntity)entry.Entity).Modified = DateTime.UtcNow;
            }
        }
    }
}