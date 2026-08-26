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
    public class StudentController : ControllerBase
    {
        // The controller knows about ONE thing: the service interface.
        // No AppDbContext. No repositories. No EF Core imports.
        private readonly IStudentService _studentService;
        private readonly JwtServices _jwtService;
        private readonly IAuthCookieService _authCookieService;

        public StudentController(
            IStudentService studentService,
            JwtServices jwtService,
            IAuthCookieService authCookieService)
        {
            _studentService = studentService;
            _jwtService = jwtService;
            _authCookieService = authCookieService;
        }

        // ── Auth endpoints (public) ───────────────────────────────────────────

        [EnableRateLimiting("auth-register")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentDto dto)
        {

            var student = await _studentService.RegisterStudent(dto);
            return Ok(new { student.Id, student.Name, student.Email, student.Role });
        }

        [EnableRateLimiting("auth-login")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var student = await _studentService.LoginStudent(dto);
            if (student is null)
                return Unauthorized("Invalid email or password.");

            var token = _jwtService.GenerateJwtToken(student.Id, student.Email, student.Role);
            _authCookieService.SetAuthCookie(Response, token);

            return Ok(new { student.Name, student.Email, student.Role });
        }

        // ── Protected student endpoints ───────────────────────────────────────

        // Shared helper: parse the student ID from the JWT claim safely.
        // Returns null if the claim is missing or not a valid Guid.
        private Guid? GetStudentId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        [Authorize(Roles = "Student")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var studentId = GetStudentId();
            if (studentId is null) return Unauthorized();

            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(new { studentId, email });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-gpa")]
        public async Task<IActionResult> MyGpa()
        {
            var studentId = GetStudentId();
            if (studentId is null) return Unauthorized();

            var records = await _studentService.GetGpaRecords(studentId.Value);
            return Ok(records);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-schedules")]
        public async Task<IActionResult> MySchedules()
        {
            var studentId = GetStudentId();
            if (studentId is null) return Unauthorized();

            var schedules = await _studentService.GetMySchedules(studentId.Value);
            return Ok(schedules);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-assignments")]
        public async Task<IActionResult> MyAssignments()
        {
            var studentId = GetStudentId();
            if (studentId is null) return Unauthorized();

            var assignments = await _studentService.GetMyAssignments(studentId.Value);
            return Ok(assignments);
        }

        // POST /api/student/assignments/{assignmentId}/submit
        // Student uploads a file (PDF/doc/etc.) for an assignment. multipart/form-data.
        [Authorize(Roles = "Student")]
        [HttpPost("assignments/{assignmentId:guid}/submit")]
        public async Task<IActionResult> SubmitAssignment(Guid assignmentId, IFormFile file)
        {
            var studentId = GetStudentId();
            if (studentId is null) return Unauthorized();

            var submission = await _studentService.SubmitAssignment(assignmentId, studentId.Value, file);
            return Ok(new
            {
                submission.Id,
                submission.AssignmentId,
                submission.FilePath,
                submission.SubmittedAt
            });


        }
    }
}
