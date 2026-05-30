using Microsoft.EntityFrameworkCore;
using CampusFlow.Data;
using CampusFlow.DTO;
using CampusFlow.Model;
namespace CampusFlow.Repositories
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

        public async Task<Student> getStudentById(string id)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(id));
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
