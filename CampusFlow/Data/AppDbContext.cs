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
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            // Account emails are normalized (trim + lowercase) in the service layer;
            // these indexes enforce uniqueness as the final authority.
            modelbuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelbuilder.Entity<Teacher>()
                .HasIndex(t => t.Email)
                .IsUnique();

            modelbuilder.Entity<Student>()
           .HasMany(s => s.StudentGPA)
           .WithOne(g => g.student)
           .HasForeignKey(g => g.StudentId);

            modelbuilder.Entity<Student>()
                .HasMany(s => s.Schedules)
                .WithOne(g => g.Student)
                .HasForeignKey(sc => sc.StudentId);

            // A teacher's assignment has many student submissions.
            modelbuilder.Entity<Assignment>()
                .HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentId);

            // A student has many submissions. Restrict on delete so the two cascade
            // paths into Submission (from Assignment and from Student) don't conflict
            // on SQL Server.
            modelbuilder.Entity<Student>()
                .HasMany(s => s.Submissions)
                .WithOne(sub => sub.Student)
                .HasForeignKey(sub => sub.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // A student can submit a given assignment only once.
            modelbuilder.Entity<Submission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();

            modelbuilder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            modelbuilder.Entity<CourseSection>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelbuilder.Entity<CourseSection>()
                .HasOne(cs => cs.Teacher)
                .WithMany(t => t.CourseSections)
                .HasForeignKey(cs => cs.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelbuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.CourseEnrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelbuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.CourseSection)
                .WithMany(cs => cs.Enrollments)
                .HasForeignKey(e => e.CourseSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelbuilder.Entity<CourseEnrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseSectionId })
                .IsUnique();

            modelbuilder.Entity<Assignment>()
                .HasOne(a => a.CourseSection)
                .WithMany(cs => cs.Assignments)
                .HasForeignKey(a => a.CourseSectionId)
                .OnDelete(DeleteBehavior.Restrict);


        }

    }
}
