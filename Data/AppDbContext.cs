using Microsoft.EntityFrameworkCore;
using TeamDesk.DTOs;
using TeamDesk.Models.Entities;

namespace TeamDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Email)
                .IsUnique();
        }
    }

}
