using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Extensions;
using Student.API.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSharedServices();

builder.Services.AddDbContext<StudentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();