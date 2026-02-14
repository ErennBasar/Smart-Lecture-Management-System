using Advisor.API.DTOs;
using Advisor.API.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Services;

namespace Advisor.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly AdvisorDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IConfiguration _configuration;

        public AdvisorsController(AdvisorDbContext dbContext, 
            IHttpClientFactory httpClientFactory, 
            IAuthenticatedUserService authenticatedUserService, 
            IPublishEndpoint publishEndpoint, 
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _authenticatedUserService = authenticatedUserService;
            _publishEndpoint = publishEndpoint;
            _configuration = configuration;
        }

        [HttpGet("get-all-advisors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdvisors()
        {
            var advisors = await _dbContext.Advisors.ToListAsync();
            
            return Ok(advisors);
        }

        [HttpGet("get-id-by-user/{userId}")]
        [Authorize(Roles = ("Admin, Academician"))]
        public async Task<IActionResult> GetAdvisorIdByUserId(Guid userId)
        {
            var advisorId = await _dbContext.Advisors
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (advisorId == Guid.Empty)
                return NotFound("Akademisyen bulunamadı");
                

            return Ok(advisorId);
        } // StudentController'daki GetMyStudents() endpoint'i içinden bu endpoint'e http isteği atılıyor.
        // Akademisyen GetMyStudents() endpoint'ine istek attığında userId'si ile bu endpoint'e geliyor ve gelen userId'ye karşılık gelen Id geri döndürülüyor
        

        [HttpPut("update-advisor/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAdvisor(Guid id,[FromBody] UpdateAdvisorDto updateAdvisorDto)
        {
            var advisor = await _dbContext.Advisors.FirstOrDefaultAsync(a => a.Id == id);

            if (advisor == null)
                return NotFound($"Bu {id} ile eşleşen bir Akademisyen bulunamadı");

            var changes = new Dictionary<string, object>();
            bool nameChanged = false;

            if (!string.IsNullOrEmpty(updateAdvisorDto.FirstName) && advisor.FirstName != updateAdvisorDto.FirstName)
            {
                advisor.FirstName = updateAdvisorDto.FirstName;
                changes.Add("First Name",updateAdvisorDto.FirstName);
                nameChanged = true;
            }
            
            if (!string.IsNullOrEmpty(updateAdvisorDto.LastName) && advisor.LastName != updateAdvisorDto.LastName)
            {
                advisor.LastName = updateAdvisorDto.LastName;
                changes.Add("Last Name",updateAdvisorDto.LastName);
                nameChanged = true;
            }

            if (!string.IsNullOrEmpty(updateAdvisorDto.Department) && advisor.Department != updateAdvisorDto.Department)
            {
                advisor.Department = updateAdvisorDto.Department;
                changes.Add("Department", updateAdvisorDto.Department);
            }

            if (!string.IsNullOrEmpty(updateAdvisorDto.Specialization) && advisor.Specialization != updateAdvisorDto.Specialization)
            {
                advisor.Specialization = updateAdvisorDto.Specialization;
                changes.Add("Specialization",updateAdvisorDto.Specialization);
            }

            if (nameChanged) // FirstName ve LastName AspNetUsers tablosunda'da old. için senkronizasyon sağlamak gerektiğinden bu kontrol yapıldı.
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();

                    var userUpdateModel = new
                    {
                        FirstName = !string.IsNullOrEmpty(updateAdvisorDto.FirstName)
                            ? updateAdvisorDto.FirstName
                            : advisor.FirstName,
                        
                        LastName = !string.IsNullOrEmpty(updateAdvisorDto.LastName)
                            ? updateAdvisorDto.LastName
                            : advisor.LastName
                    };

                    var response = await client.PutAsJsonAsync($"http://localhost:5294/api/auth/update/{advisor.UserId}",
                        userUpdateModel);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"KRİTİK HATA: Auth API güncellenemedi! Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Auth API erişim hatası: {ex.Message}");
                }
            }

            if (changes.Count == 0)
            {
                return Ok(new
                {
                    message = "Herhangi bir değişiklik yapılmadı, veriler zaten güncel."
                });
            }

            advisor.UpdatedDate = DateTime.UtcNow;

            _dbContext.Advisors.Update(advisor);
            await _dbContext.SaveChangesAsync();
            
            

            return Ok(new
            {
                Message = "Akademisyen güncellendi",
                AdvisorId = advisor.Id,
                UpdatedFields = changes
                
            });
        }

        [HttpPost("create-advisor")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> CreateAdvisor(CreateAdvisorDto createAdvisorDto)
        {
            var existingAdvisor = await _dbContext.Advisors.FirstOrDefaultAsync(a =>
                a.UserId == createAdvisorDto.UserId);

            if (existingAdvisor != null)
                return Conflict("BU Id'ye sahip danışman zaten mevcut");
            
            // Identity Servisine Sor 
            var client = _httpClientFactory.CreateClient();
            
            // http isteği atarken accessToken değişkeni ile kullanıcıya ait token'da iletiliyor.
            var accessToken = HttpContext.Request.Headers["Authorization"].ToString();
            
            if(!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Add("Authorization",accessToken);

            var requestUrl = $"http://localhost:5294/api/auth/users/{createAdvisorDto.UserId}";
            var response = await client.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
        
                return BadRequest(new 
                {
                    ErrorType = "Auth API Hatası",
                    StatusCode = response.StatusCode, 
                    TargetUrl = requestUrl,
                    AuthApiMessage = errorContent 
                });}

            var identityUser = await response.Content.ReadFromJsonAsync<IdentityUserDto>();
            
            var newAdvisor = new Models.Advisor
            {
                Id = Guid.NewGuid(),
                UserId = createAdvisorDto.UserId,
                FirstName = identityUser.FirstName,
                LastName = identityUser.LastName,
                Email = identityUser.Email,
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteAdvisor(Guid id)
        {
            var advisor = await _dbContext.Advisors.FirstOrDefaultAsync(s => s.Id == id);

            if (advisor == null)
                return NotFound("Akademisyen bulunamadı");
            
            var client = _httpClientFactory.CreateClient();

            var accessToken = HttpContext.Request.Headers["Authorization"].ToString();

            if(!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Add("Authorization", accessToken);
            
            var studentApiUrl = _configuration["ServiceUrls:StudentApi"];
            
            if (string.IsNullOrEmpty(studentApiUrl)) studentApiUrl = "http://localhost:5053";

            var requestUrl = $"{studentApiUrl}/api/students/check-active-students/{id}";
            
            var response = await client.GetAsync(requestUrl);
            
            
            if (response.IsSuccessStatusCode)
            {
                var hasStudents = await response.Content.ReadFromJsonAsync<bool>();
                if (hasStudents)
                {
                    return BadRequest("BU DANIŞMAN SİLİNEMEZ! Üzerine zimmetli aktif öğrenciler bulunmaktadır. Lütfen önce öğrencileri transfer edin.");
                }
            }
            else 
            {
                // Student API'ye ulaşılamadıysa silme işlemini durdurmak güvenlik gereğidir.
                //return StatusCode(500, "Öğrenci kontrolü yapılamadığı için silme işlemi durduruldu.");
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Student API Hatası: {response.StatusCode} - {errorContent}");
            }

            advisor.IsDeleted = true;
            advisor.DeletedDate = DateTime.UtcNow;
            advisor.IsActive = false;

            _dbContext.Advisors.Update(advisor);
            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish<IAdvisorDeletedEvent>(new
            {
                AdvisorId = advisor.Id,
                UserId = advisor.UserId
            });

            return Ok(new
            {
                Message = "Akademisyen başarıyla silindi ve pasife alındı."
            });
        }
    }
}