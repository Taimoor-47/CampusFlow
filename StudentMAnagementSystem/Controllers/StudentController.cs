using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMAnagementSystem.Data;
using StudentMAnagementSystem.DTO;
using StudentMAnagementSystem.Model;
using StudentMAnagementSystem.Repositories;
using StudentMAnagementSystem.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace StudentMAnagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IStudentRepository _studentRepository;
        private readonly JwtServicescs _jwtService;

        public StudentController(IStudentService studentService, IStudentRepository studentRepository, JwtServicescs jwtService)
        {
            _studentService = studentService;
            _studentRepository = studentRepository;
            _jwtService = jwtService;
        }
        [HttpGet]
        public async Task<IActionResult> getAllStudents()
        {
            try
            {
                var students = await _studentRepository.getAllstudents();

                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());

            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(email);
        }

        //-----------------------registring a student---------------------//
        [HttpPost("registerStudent")]
        public async Task<IActionResult> registerStudent([FromBody] StudentDto dto)
        {
            try
            {
                var students = await _studentService.registerStudent(dto);
                if (students == null)
                    return Unauthorized("Invalid email or password");

                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("loginStudent")]
        public async Task<IActionResult> loginStudent([FromBody] LoginDto dto)
        {
            var student = await _studentService.loginStudent(dto);
            if (student == null)
                return Unauthorized("Invalid email or password");

            var token = _jwtService.GenerateJwtToken(student);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                name=student.Email,
            
            });
        }
    }
}
