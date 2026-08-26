using CampusFlow.DTO;
using CampusFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly JwtServices _jwtService;
        private readonly IAuthCookieService _authCookieService;

        public TeacherController(
            ITeacherService teacherService,
            JwtServices jwtService,
            IAuthCookieService authCookieService)
        {
            _teacherService = teacherService;
            _jwtService = jwtService;
            _authCookieService = authCookieService;
        }

        // POST /api/teacher/register
        [EnableRateLimiting("auth-register")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterTeacherDto dto)
        {
            var teacher = await _teacherService.Register(dto);
            return Ok(new { teacher.Id, teacher.Name, teacher.Email, teacher.Role });
        }

        // POST /api/teacher/login
        // Same cookie-based approach as student login so protected endpoints work.
        [EnableRateLimiting("auth-login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var teacher = await _teacherService.Login(dto);
            if (teacher == null)
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateJwtToken(teacher.Id, teacher.Email, teacher.Role);
            _authCookieService.SetAuthCookie(Response, token);

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

            var gpa = await _teacherService.AddGpa(dto);
            return Ok(gpa);
        }

        // POST /api/teacher/add-schedule
        // Teacher adds a class schedule entry for a student.
        [Authorize(Roles = "Teacher")]
        [HttpPost("add-schedule")]
        public async Task<IActionResult> AddSchedule([FromBody] AddScheduleDto dto)
        {
            var schedule = await _teacherService.AddSchedule(dto);
            return Ok(schedule);
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

        [Authorize(Roles = "Teacher")]
        [HttpGet("assignments/{assignmentId:guid}/submissions")]
        public async Task<IActionResult> GetSubmissions(Guid assignmentId)
        {
            var teacherId = GetTeacherId();

            if (teacherId is null)
            {
                return Unauthorized();
            }

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

        // Parse the teacher's ID from the JWT claim; null if missing or malformed.
        private Guid? GetTeacherId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        // GET /api/teacher/me
        // Server-verified identity for the signed-in teacher; lets the frontend
        // validate its cookie-backed session instead of trusting sessionStorage.
        [Authorize(Roles = "Teacher")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var teacherId = GetTeacherId();
            if (teacherId is null) return Unauthorized();

            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(new { teacherId, email });
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
