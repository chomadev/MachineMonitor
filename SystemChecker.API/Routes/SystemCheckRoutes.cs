using SystemChecker.API.Models.Dto;
using SystemChecker.API.Services;

namespace SystemChecker.API.Routes;

public static class SystemCheckRoutes
{
    public static void MapSystemCheckRoutes(this WebApplication app)
    {
        app.MapPost("/api/systemcheck", async (
            string apiKey,
            SystemCheckDto check,
            ISystemCheckHistoryService service) =>
        {
            try
            {
                var result = await service.SaveCheckAsync(apiKey, check);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("SaveSystemCheck")
        .WithOpenApi()
        .WithTags("SystemCheck");

        app.MapGet("/api/systemcheck/latest", async (
            string apiKey,
            ISystemCheckHistoryService service) =>
        {
            try
            {
                var result = await service.GetLatestCheckAsync(apiKey);
                return result != null ? Results.Ok(result) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("GetLatestSystemCheck")
        .WithOpenApi()
        .WithTags("SystemCheck");

        app.MapGet("/api/systemcheck/history", async (
            string apiKey,
            DateTime? from,
            DateTime? to,
            ISystemCheckHistoryService service) =>
        {
            try
            {
                var result = await service.GetCheckHistoryAsync(apiKey, from, to);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("GetSystemCheckHistory")
        .WithOpenApi()
        .WithTags("SystemCheck");
    }
} 