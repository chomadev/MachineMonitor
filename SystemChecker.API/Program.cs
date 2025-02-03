using SystemChecker.API.Models;
using SystemChecker.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISystemCheckStore, InMemorySystemCheckStore>();

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
app.MapPost("/api/systemcheck", async (SystemCheckData check, ISystemCheckStore store) =>
{
    await store.SaveCheckAsync(check);
    return Results.Ok();
})
.WithName("SaveSystemCheck")
.WithOpenApi()
.AllowAnonymous(); // Permite acesso anônimo

app.MapGet("/api/systemcheck/latest", async (ISystemCheckStore store) =>
{
    var latestCheck = await store.GetLatestCheckAsync();
    if (latestCheck == null)
        return Results.NotFound();
        
    return Results.Ok(latestCheck);
})
.WithName("GetLatestSystemCheck")
.WithOpenApi()
.AllowAnonymous(); // Permite acesso anônimo

app.Run(); 