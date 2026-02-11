using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Services;
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
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public StudentsController(StudentDbContext dbContext, 
            IHttpClientFactory httpClientFactory, 
            IAuthenticatedUserService authenticatedUserService)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _authenticatedUserService = authenticatedUserService;
        }

        [HttpGet("get-all")]
        [Authorize(Roles = ("Admin, Academician"))]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _dbContext.Students.ToListAsync();
            
            return Ok(students);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentInfoById()
        {
            var studentId = _authenticatedUserService.UserId;
            
            if (studentId == Guid.Empty)
                return Unauthorized();

            var studentInfo = await _dbContext.Students
                .Where(w => w.UserId == studentId)
                .Select(e => new
                {
                    e.Address,
                    e.StudentNumber,
                    e.DateOfBirth,
                    e.Email,
                    e.FirstName,
                    e.LastName,
                    e.IsActive,

                }).FirstOrDefaultAsync();

            if (studentInfo == null)
                return NotFound("Öğrencinin profil bilgilerine ulaşılamadı");

            return Ok(studentInfo);
        }

        [HttpPost("create-student")]
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
        
        [HttpPut("update-student/{id}")]
        [Authorize(Roles = ("Admin, Academician"))]
        public async Task<IActionResult> UpdateStudent(Guid id,[FromBody] UpdateStudentDto updateDto)
        {
            var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound($"HATA:\n '{id}' Bu ID ile eşleşen bir öğrenci bulunmamaktadır!");

            var changes = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(updateDto.FirstName) && student.FirstName != updateDto.FirstName)
            {
                student.FirstName = updateDto.FirstName;
                changes.Add("FirstName",updateDto.FirstName);
            }

            if (!string.IsNullOrEmpty(updateDto.LastName) && student.LastName != updateDto.LastName)
            {
                student.LastName = updateDto.LastName;
                changes.Add("LastName",updateDto.LastName);
            }

            if (updateDto.AdvisorId.HasValue && updateDto.AdvisorId
                != Guid.Empty && student.AdvisorId
                != updateDto.AdvisorId)
            {
                student.AdvisorId = updateDto.AdvisorId.Value;
                changes.Add("AdvisorId",updateDto.AdvisorId.Value);
            }


            if (!string.IsNullOrEmpty(updateDto.Address) && student.Address != updateDto.Address)
            {
                student.Address = updateDto.Address;
                changes.Add("Address",updateDto.Address);
            }

            if (updateDto.IsActive.HasValue && student.IsActive != updateDto.IsActive.Value)
            {
                student.IsActive = updateDto.IsActive.Value;
                changes.Add("IsActive",updateDto.IsActive.Value);
            }

            if (updateDto.DateOfBirth.HasValue)
            {
                var newDate = updateDto.DateOfBirth.Value.ToUniversalTime();

                if (newDate != student.DateOfBirth)
                {
                    student.DateOfBirth = newDate;
                    changes.Add("DateOfBirth",newDate);
                }
            }
            
            // Eğer hiçbir değişiklik yapılmadıysa 
            if (changes.Count == 0)
            {
                return Ok(new
                {
                    message = "Herhangi bir değişiklik yapılmadı, veriler zaten güncel."
                });
            }

            student.UpdatedDate = DateTime.UtcNow;

            _dbContext.Students.Update(student);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Güncelleme işlemi tamamlandı",
                StudentId = student.Id,
                UpdatedFields = changes
            });
        }

        [HttpGet("advisor/my-students")]
        [Authorize(Roles = "Academician")]
        public async Task<IActionResult> GetMyStudents()
        {
            var advisorUserId = _authenticatedUserService.UserId;

            if (advisorUserId == Guid.Empty)
                return NotFound("Bu Id'ye sahip bir Akademisyen yok");

            var client = _httpClientFactory.CreateClient();
            var requestUrl = $"http://localhost:5053/api/advisors/get-id-by-user/{advisorUserId}";
            var response = await client.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"--- HATA DETAYI ---");
                Console.WriteLine($"Gidilen Adres: {requestUrl}");
                Console.WriteLine($"Hata Kodu: {response.StatusCode}");
                Console.WriteLine($"Hata Mesajı: {await response.Content.ReadAsStringAsync()}");
        
                return BadRequest($"Akademisyen bilgisine ulaşılamadı. Hata Kodu: {response.StatusCode}");}

            var advisorIdString = await response.Content.ReadAsStringAsync();

            var advisorId = Guid.Parse(advisorIdString.Trim('"'));

            var students = await _dbContext.Students
                .Where(s => s.AdvisorId == advisorId & s.IsActive)
                .Select(s => new
                {
                    s.StudentNumber,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.Address,
                    s.DateOfBirth,
                    s.EnrollmentDate,

                }).ToListAsync();

            return Ok(students);

        }
    }
}