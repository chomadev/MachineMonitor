using SystemChecker.API.Services;
using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Data;
using SystemChecker.API.Routes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<ISystemCheckHistoryService, SystemCheckHistoryService>();

// Add CORS
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

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SystemChecker API v1");
        c.RoutePrefix = "swagger";
    });
}
else 
{
    app.UseHttpsRedirection();
}

// Use CORS
app.UseCors("AllowAll");

// Map routes
app.MapSystemCheckRoutes();
app.MapMachineRoutes();

app.Run(); 