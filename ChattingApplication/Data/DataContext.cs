using ChattingApplication.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChattingApplication.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) 
        {
            
        }

        public DbSet<AppUser> Users { get; set; }
    }
}
