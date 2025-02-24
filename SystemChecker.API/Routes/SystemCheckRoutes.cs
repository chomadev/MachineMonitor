using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SystemChecker.API.Models;
using SystemChecker.API.Services;

namespace SystemChecker.API.Routes;

public static class SystemCheckRoutes
{
    public static void MapSystemCheckRoutes(this WebApplication app)
    {
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
            catch (Exception ex) {
                return Results.BadRequest();
            }
        })
        .WithName("SaveSystemCheck")
        .WithOpenApi()
        .WithTags("SystemCheck");

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
        .WithOpenApi()
        .WithTags("SystemCheck");

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
        .WithOpenApi()
        .WithTags("SystemCheck");
    }
} 