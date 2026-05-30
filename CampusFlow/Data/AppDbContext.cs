using CampusFlow.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CampusFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<StudentGPA> StudentGPA { get; set; }
        public DbSet<Schedules> Schedules { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            modelbuilder.Entity<Student>()
           .HasMany(s => s.StudentGPA)
           .WithOne(g => g.student)
           .HasForeignKey(g => g.StudentId);

            modelbuilder.Entity<Student>()
                .HasMany(s => s.Schedules)
                .WithOne(g => g.Student)
                .HasForeignKey(sc => sc.StudentId);

            modelbuilder.Entity<Student>()
                .HasMany(s => s.Assignments)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId);
        }

    }
}
