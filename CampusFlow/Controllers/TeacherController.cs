using CampusFlow.DTO;
using CampusFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly JwtServicescs _jwtService;

        public TeacherController(ITeacherService teacherService, JwtServicescs jwtService)
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

        // POST /api/teacher/add-assignment
        // Teacher creates an assignment for a student.
        [Authorize(Roles = "Teacher")]
        [HttpPost("add-assignment")]
        public async Task<IActionResult> AddAssignment([FromBody] AddAssignmentDto dto)
        {
            try
            {
                var assignment = await _teacherService.AddAssignment(dto);
                return Ok(assignment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
