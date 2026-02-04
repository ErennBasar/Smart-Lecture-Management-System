using Lecture.API.DTOs;
using Lecture.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lecture.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly LectureDbContext _dbContext;

        public LecturesController(LectureDbContext dbContext)
        {
            _dbContext = dbContext;
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
    }
}
