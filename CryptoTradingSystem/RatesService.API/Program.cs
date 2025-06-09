using MediatR;
using Microsoft.EntityFrameworkCore;
using RatesService.Application.Handlers;
using RatesService.Application.Services;
using RatesService.Infra.Persistence;
using Events;
using Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RatesDbContext>(options =>
    options.UseInMemoryDatabase("RatesDb"));

builder.Services.AddHttpClient<ICoinMarketCapClient, CoinMarketCapClient>(client =>
{
    client.BaseAddress = new Uri("https://pro-api.coinmarketcap.com/");
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FetchRatesHandler).Assembly));

builder.Services.AddCrossMicroserviceMessaging();

if (builder.Configuration.GetValue<bool>("Scheduler:Enabled", false))
{
    builder.Services.AddHostedService<RatesService.API.Services.RateSchedulerService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/rates/fetch", async (IMediator mediator) =>
    {
        await mediator.Send(new FetchRatesCommand());
        return Results.Ok(new { message = "Rates fetch initiated" });
    })
    .WithName("FetchRates")
    .WithOpenApi();

app.MapGet("/api/rates/recent/{symbol}", async (string symbol, RatesDbContext context) =>
    {
        var rates = await context.Rates
            .Where(r => r.Symbol == symbol)
            .OrderByDescending(r => r.Timestamp)
            .Take(10)
            .Select(r => new
            {
                r.Symbol,
                r.Value,
                r.Timestamp
            })
            .ToListAsync();
    
        return Results.Ok(rates);
    })
    .WithName("GetRecentRates")
    .WithOpenApi();

app.Run();