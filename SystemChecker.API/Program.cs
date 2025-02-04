using SystemChecker.API.Models;
using SystemChecker.API.Services;
using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ISystemCheckHistoryService, SystemCheckHistoryService>();

// Adiciona CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

// Usa CORS antes do UseHttpsRedirection
app.UseCors("AllowAll");

// Endpoints
app.MapPost("/api/systemcheck", async (
    SystemCheckData check,
    string apiKey,
    ISystemCheckHistoryService historyService) =>
{
    try
    {
        var history = await historyService.SaveCheckAsync(apiKey, check);
        return Results.Ok(history);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("SaveSystemCheck")
.WithOpenApi();

app.MapGet("/api/systemcheck/latest", async (
    string apiKey,
    ISystemCheckHistoryService historyService) =>
{
    try
    {
        var latestCheck = await historyService.GetLatestCheckAsync(apiKey);
        if (latestCheck == null)
            return Results.NotFound();
            
        return Results.Ok(latestCheck);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetLatestSystemCheck")
.WithOpenApi();

app.MapGet("/api/systemcheck/history", async (
    string apiKey,
    DateTime? from,
    DateTime? to,
    ISystemCheckHistoryService historyService) =>
{
    try
    {
        var history = await historyService.GetCheckHistoryAsync(apiKey, from, to);
        return Results.Ok(history);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetSystemCheckHistory")
.WithOpenApi();

app.MapPost("/api/keys", async (
    IApiKeyService keyService,
    string machineName,
    string? description) =>
{
    var apiKey = await keyService.GenerateApiKeyAsync(machineName, description);
    return Results.Ok(new { apiKey.Key, MachineName = apiKey.Machine.Name });
})
.WithName("GenerateApiKey")
.WithOpenApi();

app.Run(); 