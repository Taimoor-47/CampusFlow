using CampusFlow.DTO;
using CampusFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly JwtServicescs _jwtService;

        public StudentController(IStudentService studentService, JwtServicescs jwtService)
        {
            _studentService = studentService;
            _jwtService = jwtService;
        }

        // POST /api/student/register
        // Anyone can register a new student account.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentDto dto)
        {
            try
            {
                var student = await _studentService.RegisterStudent(dto);
                return Ok(new { student.Id, student.Name, student.Email, student.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/student/login
        // Returns a JWT token stored in an HTTP-only cookie.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var student = await _studentService.LoginStudent(dto);
            if (student == null)
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateJwtToken(student.Id, student.Email, student.Role);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,        // JS cannot read it — prevents XSS theft
                Secure = true,          // Only sent over HTTPS
                SameSite = SameSiteMode.None,  // Needed for cross-port requests (e.g. :3000 → :7288)
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { student.Name, student.Email, student.Role });
        }

        // GET /api/student/me
        // Returns the profile of the currently logged-in student.
        [Authorize(Roles = "Student")]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentId == null) return Unauthorized();

            // We re-use the repository through the service — fetch the full student record.
            var schedules = await _studentService.GetMySchedules(Guid.Parse(studentId));
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new { email, studentId });
        }

        // GET /api/student/my-gpa
        // A student can only see their OWN GPA records.
        [Authorize(Roles = "Student")]
        [HttpGet("my-gpa")]
        public async Task<IActionResult> MyGpa([FromServices] CampusFlow.Data.AppDbContext context)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentId == null) return Unauthorized();

            var gpa = await context.StudentGPA
                .Where(g => g.StudentId == Guid.Parse(studentId))
                .OrderBy(g => g.Semester)
                .ToListAsync();

            return Ok(gpa);
        }

        // GET /api/student/my-schedules
        // A student can only see their OWN class schedule.
        [Authorize(Roles = "Student")]
        [HttpGet("my-schedules")]
        public async Task<IActionResult> MySchedules()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentId == null) return Unauthorized();

            var schedules = await _studentService.GetMySchedules(Guid.Parse(studentId));
            return Ok(schedules);
        }

        // GET /api/student/my-assignments
        // A student can only see assignments assigned to them.
        [Authorize(Roles = "Student")]
        [HttpGet("my-assignments")]
        public async Task<IActionResult> MyAssignments()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentId == null) return Unauthorized();

            var assignments = await _studentService.GetMyAssignments(Guid.Parse(studentId));
            return Ok(assignments);
        }
    }
}
