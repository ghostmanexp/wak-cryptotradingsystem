using MediatR;
using Microsoft.EntityFrameworkCore;
using PositionsService.Application.Handlers;
using PositionsService.Application.Services;
using PositionsService.Infra.Persistence;
using Events;
using Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PositionsDbContext>(options =>
    options.UseInMemoryDatabase("PositionsDb"));

builder.Services.AddScoped<ICsvPositionLoader, CsvPositionLoader>();

builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(AddPositionHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RateChangedEvent).Assembly);
});

builder.Services.AddCrossMicroserviceMessaging();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var csvLoader = scope.ServiceProvider.GetRequiredService<ICsvPositionLoader>();
    var csvPath = builder.Configuration["CsvFilePath"] ?? "positions.csv";
    
    if (File.Exists(csvPath))
    {
        await csvLoader.LoadPositionsFromCsvAsync(csvPath);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/positions", async (AddPositionCommand command, IMediator mediator) =>
{
    var positionId = await mediator.Send(command);
    return Results.Created($"/api/positions/{positionId}", new { id = positionId });
})
.WithName("AddPosition")
.WithOpenApi();

app.MapDelete("/api/positions/{id}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new ClosePositionCommand { PositionId = id });
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("ClosePosition")
.WithOpenApi();

app.MapGet("/api/positions", async (PositionsDbContext context) =>
{
    var positions = await context.Positions
        .Where(p => p.Status == PositionsService.Domain.Entities.PositionStatus.Open)
        .Select(p => new
        {
            p.Id,
            p.InstrumentId,
            p.Quantity,
            p.InitialRate,
            p.Side,
            p.CreatedAt
        })
        .ToListAsync();
    
    return Results.Ok(positions);
})
.WithName("GetOpenPositions")
.WithOpenApi();

app.Run();