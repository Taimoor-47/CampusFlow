using Microsoft.EntityFrameworkCore;
using StudentMAnagementSystem.Model;

namespace StudentMAnagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
