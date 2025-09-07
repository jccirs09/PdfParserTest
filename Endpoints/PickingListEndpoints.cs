using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PickingListApp.DTOs;
using PickingListApp.Services;

namespace PickingListApp.Endpoints;

public static class PickingListEndpoints
{
    public static void MapPickingListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pickinglists");

        group.MapPost("/upload", async (IFormFile file, IPickingListParser parser) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                return Results.BadRequest("Invalid file type. Please upload a PDF.");
            }

            await using var stream = file.OpenReadStream();
            var parsedList = await parser.Parse(stream);

            return Results.Ok(parsedList);
        })
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<PickingListDto>()
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", async (int id, PickingListService service) =>
        {
            var pickingList = await service.GetPickingListByIdAsync(id);
            return pickingList == null ? Results.NotFound() : Results.Ok(pickingList);
        })
        .Produces<PickingListDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async ([FromQuery] string soNumber, PickingListService service) =>
        {
            if (string.IsNullOrWhiteSpace(soNumber))
            {
                return Results.BadRequest("soNumber is required.");
            }
            var pickingList = await service.GetPickingListBySoNumberAsync(soNumber);
            return pickingList == null ? Results.NotFound() : Results.Ok(pickingList);
        })
        .Produces<PickingListDto>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (PickingListDto pickingListDto, PickingListService service) =>
        {
            var (id, salesOrderNumber) = await service.SavePickingListAsync(pickingListDto);
            return Results.Created($"/api/pickinglists/{id}", new { id, salesOrderNumber });
        })
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }
}
