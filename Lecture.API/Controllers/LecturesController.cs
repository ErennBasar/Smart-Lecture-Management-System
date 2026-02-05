using Lecture.API.DTOs;
using Lecture.API.Models;
using Lecture.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace Lecture.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly LectureDbContext _dbContext;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public LecturesController(LectureDbContext dbContext, 
            IAuthenticatedUserService authenticatedUserService)
        {
            _dbContext = dbContext;
            _authenticatedUserService = authenticatedUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLectures()
        {
            var lectures = await _dbContext.Lectures.ToListAsync();
            return Ok(lectures);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLecture(CreateLectureDto createLectureDto)
        {
            var newLecture = new Models.Entities.Lecture
            {
                Id = Guid.NewGuid(),
                AdvisorId = createLectureDto.AdvisorId,
                CourseCode = createLectureDto.CourseCode,
                CourseName = createLectureDto.CourseName,
                Description = createLectureDto.Description,
                Credits = createLectureDto.Credits,
                MaxStudents = createLectureDto.MaxStudents,
                StartDate = createLectureDto.StartDate,
                EndDate = createLectureDto.EndDate,
                Status = Shared.Enums.LectureStatus.NotStarted,
            };

            _dbContext.Lectures.AddAsync(newLecture);
            await _dbContext.SaveChangesAsync();

            return Ok( new
            {
                message = "Ders oluşturuldu.", lectureId = newLecture.Id
            });
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody]Guid lectureId)
        {
            // 1. Shared Servisinden Id'yi alıyoruz 
            
            var studentId = _authenticatedUserService.UserId;
            
            if (studentId == Guid.Empty)
                return Unauthorized("Kullanıcı kimliği doğrulanamadı");
            
            // 2. Ders var mı?
            
            var lecture = await _dbContext.Lectures.FindAsync(lectureId);
            
            if (lecture == null)
                return NotFound("Ders Bulunamadı !");
            
            // 3. Zaten kayıtlı mı
            
            var alreadyEnrolled = await _dbContext.CourseEnrollments.AnyAsync(c =>
                c.LectureId == lectureId && c.StudentId == studentId);
            
            if (alreadyEnrolled != null)
                return BadRequest("Derse Zaten Kayıtlısınız !");
            
            // 4. Kontenjan var mı?
            
            var currentCount = await _dbContext.CourseEnrollments.CountAsync(c =>
                c.LectureId == lectureId);
            
            if (currentCount >= lecture.MaxStudents)
                return BadRequest("Geç Kaldınız ... Ders kontenjanı DOLU !");
            
            // 5. Kaydı yap
            
            var enrollment = new CourseEnrollment()
            {
                Id = Guid.NewGuid(),
                LectureId = lectureId,
                StudentId = studentId,
                EnrollmentDate = DateTime.UtcNow
            };
            
            _dbContext.CourseEnrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();
            
            return Ok(new
            {
                message = "Derse Başarıyla Kayıt Oldunuz",
                EnrollmentId = enrollment.Id
            });
        }

        [HttpGet("my-enrollments")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var studentId = _authenticatedUserService.UserId;
            
            if (studentId == Guid.Empty)
                return Unauthorized();

            var enrollments = await _dbContext.CourseEnrollments
                .Include(e => e.Lecture)
                .Where(e => e.StudentId == studentId)
                .Select(e => new
                {
                    e.Lecture.Id,
                    e.Lecture.CourseCode,
                    e.Lecture.CourseName,
                    e.Lecture.Credits,
                    e.EnrollmentDate,
                    
                }).ToListAsync();

            return Ok(enrollments);
        }
    }
}
