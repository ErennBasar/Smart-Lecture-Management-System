using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student.API.DTOs;
using Student.API.Models;

namespace Student.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly StudentDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentsController(StudentDbContext dbContext, 
            IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Tebrikler! Pasaportun geçerli, içeri girdin.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Academician")]
        public async Task<IActionResult> CreateStudent(CreateStudentDto createStudentDto)
        {
            var existingStudent = await _dbContext.Students.FirstOrDefaultAsync(s =>
                s.UserId == createStudentDto.UserId);

            if (existingStudent != null)
            {
                return Conflict("Bu öğrenci zaten kayıtlı");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5294/api/auth/users/{createStudentDto.UserId}");

            if (!response.IsSuccessStatusCode)
                return BadRequest($"HATA: '{createStudentDto.UserId}'ID'sine sahip kullanıcı bulunamadı. Aktif olabilmesi için sisteme kayıt olması gerekmekte.");
            
            var newStudent = new Models.Student
            {
                Id = Guid.NewGuid(),
                UserId = createStudentDto.UserId,
                AdvisorId = createStudentDto.AdvisorId,
                StudentNumber = createStudentDto.StudentNumber,
                FirstName = createStudentDto.FirstName,
                LastName = createStudentDto.LastName,
                Email = createStudentDto.Email,
                DateOfBirth = createStudentDto.DateOfBirth.ToUniversalTime(),
                Address = createStudentDto.Address
            };

            _dbContext.Students.Add(newStudent);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Öğrenci başarıyla kaydedildi",
                StudentId = newStudent.Id,
                AdvisorId = newStudent.AdvisorId
            });
        }
    }
}