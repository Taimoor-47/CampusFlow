using Microsoft.EntityFrameworkCore;
using CampusFlow.Model;

namespace CampusFlow.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
