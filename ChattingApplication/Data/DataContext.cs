using ChattingApplication.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChattingApplication.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                         .Property(p => p.DateOfBirth)
                         .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),  // Convert DateOnly to DateTime
                d => DateOnly.FromDateTime(d)); 
        }
        public DbSet<AppUser> Users { get; set; }
    }
}
