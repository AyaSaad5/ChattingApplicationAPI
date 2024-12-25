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

            modelBuilder.Entity<UserLike>()
                        .HasKey(s => new { s.SourceUserId,s.TargetUserId });

            modelBuilder.Entity<UserLike>()
                         .HasOne(s => s.SourceUser)
                         .WithMany(l => l.LikedUsers)
                         .HasForeignKey(s => s.SourceUserId)
                         .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>()
                         .HasOne(t => t.TargetUser)
                         .WithMany(l => l.LikedByUsers)
                         .HasForeignKey(t => t.TargetUserId)
                         .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                         .HasOne(m => m.Recipient)
                         .WithMany(m => m.MessagesReceived)
                         .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                        .HasOne(m => m.Sender)
                        .WithMany(m => m.MessagesSent)
                        .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Message { get; set; }
    }
}
