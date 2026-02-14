using Identity.API.Models.Entities;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Shared.Events;

namespace Identity.API.Consumers;

public class AdvisorDeletedConsumer : IConsumer<IAdvisorDeletedEvent>
{
    private readonly UserManager<User> _userManager;

    public AdvisorDeletedConsumer(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<IAdvisorDeletedEvent> context)
    {
        var userId = context.Message.UserId;
        Console.WriteLine($"RabbitMQ Mesajı Alındı; Akademisyen silindi, UserID: {userId} pasife çekiliyor...");

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user != null)
        {
            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedDate = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            Console.WriteLine($"Kullanıcı (ID: {userId}) başarıyla pasife çekildi. ✅");
        }
        else
        {
            Console.WriteLine($"UYARI: UserID: {userId} ile eşleşen kullanıcı bulunamadı! ❌");
        }
    }
}