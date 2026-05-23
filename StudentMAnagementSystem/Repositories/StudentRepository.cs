using Microsoft.EntityFrameworkCore;
using StudentMAnagementSystem.Data;
using StudentMAnagementSystem.DTO;
using StudentMAnagementSystem.Model;
namespace StudentMAnagementSystem.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;


        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> getAllstudents()
        {

            List<Student> _student = await _context.Students.ToListAsync();
            return _student;
        }

        public Task<Student> getStudentById(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Student> RegisterStudent(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
            
        }

        public async Task<Student?> GetByEmail(string email)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);
        }

    }
}
