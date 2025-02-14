using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SystemChecker.API.Services;

namespace SystemChecker.API.Routes;

public static class MachineRoutes
{
    public static void MapMachineRoutes(this WebApplication app)
    {
        app.MapPost("/api/machines", async (
            IMachineService machineService,
            string machineName,
            string? description) =>
        {
            var result = await machineService.RegisterMachineAsync(machineName, description);
            return Results.Ok(new { result.ApiKey, MachineName = result.MachineName });
        })
        .WithName("RegisterMachine")
        .WithOpenApi()
        .WithTags("Machines");

        app.MapGet("/api/machines", async (
            IMachineService machineService) =>
        {
            var machines = await machineService.GetAllMachinesAsync();
            return Results.Ok(machines.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                apiKey = m.ApiKey?.Key
            }));
        })
        .WithName("GetAllMachines")
        .WithOpenApi()
        .WithTags("Machines");

        app.MapGet("/api/machines/{machineId}", async (
            IMachineService machineService,
            int machineId) =>
        {
            var machine = await machineService.GetMachineByIdAsync(machineId);
            if (machine == null)
                return Results.NotFound();
                
            return Results.Ok(machine);
        })
        .WithName("GetMachineById")
        .WithOpenApi()
        .WithTags("Machines");
    }
} 