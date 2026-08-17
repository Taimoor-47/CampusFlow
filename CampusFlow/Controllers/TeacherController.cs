using CampusFlow.DTO;
using CampusFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly JwtServices _jwtService;

        public TeacherController(ITeacherService teacherService, JwtServices jwtService)
        {
            _teacherService = teacherService;
            _jwtService = jwtService;
        }

        // POST /api/teacher/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterTeacherDto dto)
        {
            var teacher = await _teacherService.Register(dto);
            return Ok(new { teacher.Id, teacher.Name, teacher.Email, teacher.Role });
        }

        // POST /api/teacher/login
        // Same cookie-based approach as student login so protected endpoints work.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var teacher = await _teacherService.Login(dto);
            if (teacher == null)
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateJwtToken(teacher.Id, teacher.Email, teacher.Role);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { teacher.Name, teacher.Email, teacher.Role });
        }

        // GET /api/teacher/students
        // Only teachers can see the full student list.
        [Authorize(Roles = "Teacher")]
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _teacherService.GetAllStudents();
            return Ok(students);
        }

        // POST /api/teacher/add-gpa
        // Teacher assigns a GPA to a student for a specific semester.
        [Authorize(Roles = "Teacher")]
        [HttpPost("add-gpa")]
        public async Task<IActionResult> AddGpa([FromBody] AddGpaDto dto)
        {
            try
            {
                var gpa = await _teacherService.AddGpa(dto);
                return Ok(gpa);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST /api/teacher/add-schedule
        // Teacher adds a class schedule entry for a student.
        [Authorize(Roles = "Teacher")]
        [HttpPost("add-schedule")]
        public async Task<IActionResult> AddSchedule([FromBody] AddScheduleDto dto)
        {
            try
            {
                var schedule = await _teacherService.AddSchedule(dto);
                return Ok(schedule);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST /api/teacher/upload-assignment
        // Teacher creates an assignment for the class, optionally attaching a brief file
        // (PDF/doc). Sent as multipart/form-data because of the file part.
        [Authorize(Roles = "Teacher")]
        [HttpPost("assignments")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateAssignment(
        [FromForm] AddAssignmentDto dto)
        {
            var teacherId = GetTeacherId();

            if (teacherId is null)
                return Unauthorized();

            try
            {
                var assignment = await _teacherService.AddAssignment(
                    dto,
                    teacherId.Value);

                return Ok(new
                {
                    assignment.Id,
                    assignment.CourseSectionId,
                    assignment.CourseSection.Course.CourseCode,
                    assignment.CourseSection.Course.CourseTitle,
                    assignment.CourseSection.SectionName,
                    assignment.Title,
                    assignment.Description,
                    assignment.DueDate,
                    assignment.FilePath
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("assignments/{assignmentId:guid}/submissions")]
        public async Task<IActionResult> GetSubmissions(Guid assignmentId)
        {
            var teacherId = GetTeacherId();

            if (teacherId is null)
            {
                return Unauthorized();
            }

            try
            {
                var submissions =
                    await _teacherService.GetSubmissions(assignmentId, teacherId.Value);

                return Ok(submissions.Select(s => new
                {
                    s.Id,
                    s.AssignmentId,
                    s.StudentId,
                    StudentName = s.Student?.Name,
                    s.FilePath,
                    s.SubmittedAt
                }));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Parse the teacher's ID from the JWT claim; null if missing or malformed.
        private Guid? GetTeacherId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
        [Authorize(Roles = "Teacher")]
        [HttpGet("sections")]
        public async Task<IActionResult> GetMySections()
        {
            var teacherId = GetTeacherId();

            if (teacherId is null)
                return Unauthorized();

            var sections = await _teacherService.GetMySections(
                teacherId.Value);

            return Ok(sections);
        }
    }
}
