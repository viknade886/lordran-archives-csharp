using Microsoft.EntityFrameworkCore;
using lordran_archives.Models;

namespace lordran_archives.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .HasOne(i => i.SubmittedBy)
                .WithMany(u => u.Items)
                .HasForeignKey(i => i.SubmittedById);
        }
    }
}