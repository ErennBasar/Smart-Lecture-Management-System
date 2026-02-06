using Advisor.API.DTOs;
using Advisor.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Advisor.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly AdvisorDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdvisorsController(AdvisorDbContext dbContext, 
            IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Merhaba, ben Danışman Servisi. Pasaportun sağlam, içeri girdin.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Sadece rolü admin olanlar erişebilir
        public async Task<IActionResult> CreateAdvisor(CreateAdvisorDto createAdvisorDto)
        {
            var existingAdvisor = await _dbContext.Advisors.FirstOrDefaultAsync(a =>
                a.UserId == createAdvisorDto.UserId);

            if (existingAdvisor != null)
                return Conflict("BU Id'ye sahip danışman zaten mevcut");

            // Identity Servisine Sor 
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5294/api/auth/users/{createAdvisorDto.UserId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest($"HATA: '{createAdvisorDto.UserId}' ID'sine sahip bir kullanıcı Identity sisteminde bulunamadı. Önce kullanıcıyı oluşturun.");
            }
            
            var newAdvisor = new Models.Advisor
            {
                Id = Guid.NewGuid(),
                UserId = createAdvisorDto.UserId,
                FirstName = createAdvisorDto.FirstName,
                LastName = createAdvisorDto.LastName,
                Email = createAdvisorDto.Email,
                EmployeeNumber = createAdvisorDto.EmployeeNumber,
                Department = createAdvisorDto.Department,
                Specialization = createAdvisorDto.Specialization
            };

            _dbContext.Advisors.Add(newAdvisor);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Danışman başarıyla hoca oluşturuldu.",
                advisorId = newAdvisor.Id
            });
        }
    }
}
