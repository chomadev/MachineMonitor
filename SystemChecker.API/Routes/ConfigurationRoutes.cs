using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SystemChecker.API.Models;
using SystemChecker.API.Services;

namespace SystemChecker.API.Routes;

public static class ConfigurationRoutes
{
    public static void MapConfigurationRoutes(this WebApplication app)
    {
        app.MapGet("/api/configuration", async (
            string apiKey,
            IConfigurationService configService) =>
        {
            try
            {
                var config = await configService.GetConfigurationAsync(apiKey);
                return config != null ? Results.Ok(config) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("GetConfiguration")
        .WithOpenApi()
        .WithTags("Configuration");

        app.MapPut("/api/configuration", async (
            string apiKey,
            MachineConfiguration configuration,
            IConfigurationService configService) =>
        {
            try
            {
                var updated = await configService.SaveConfigurationAsync(apiKey, configuration);
                return Results.Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex);
            }
        })
        .WithName("UpdateConfiguration")
        .WithOpenApi()
        .WithTags("Configuration");
    }
} 