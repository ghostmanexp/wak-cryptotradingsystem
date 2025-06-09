using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RatesService.Domain.Entities;
using RatesService.Infra.Persistence;
using PositionsService.Domain.Entities;
using PositionsService.Infra.Persistence;
using Events;

public class IntegrationTests
{
    [Fact]
    public async Task RateChange_Should_UpdatePositionValues()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add EF Core contexts
        services.AddDbContext<RatesDbContext>(opt => opt.UseInMemoryDatabase("TestRatesDb"));
        services.AddDbContext<PositionsDbContext>(opt => opt.UseInMemoryDatabase("TestPositionsDb"));
        
        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RateChangedEvent).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(PositionsService.Application.Handlers.RateChangedHandler).Assembly);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var positionsContext = serviceProvider.GetRequiredService<PositionsDbContext>();
        
        // Create test position
        var position = new Position("BTC/USD", 1.5m, 50000m, "BUY");
        positionsContext.Positions.Add(position);
        await positionsContext.SaveChangesAsync();
        
        // Act - Publish rate change event
        await mediator.Publish(new RateChangedEvent
        {
            InstrumentId = "BTC/USD",
            OldRate = 50000m,
            NewRate = 55000m,
            PercentageChange = 10m,
            Timestamp = DateTime.UtcNow
        });
        
        // Assert
        var profitLoss = position.CalculateProfitLoss(55000m);
        Assert.Equal(7500m, profitLoss); // 1.5 * (55000 - 50000) * 1
    }
    
    [Fact]
    public void Position_ProfitLoss_Calculation_Buy()
    {
        var position = new Position("BTC/USD", 2m, 40000m, "BUY");
        var profitLoss = position.CalculateProfitLoss(45000m);
        Assert.Equal(10000m, profitLoss); // 2 * (45000 - 40000) * 1
    }
    
    [Fact]
    public void Position_ProfitLoss_Calculation_Sell()
    {
        var position = new Position("BTC/USD", 2m, 40000m, "SELL");
        var profitLoss = position.CalculateProfitLoss(45000m);
        Assert.Equal(-10000m, profitLoss); // 2 * (45000 - 40000) * -1
    }
    
    [Fact]
    public void Rate_SignificantChange_Detection()
    {
        var oldRate = new Rate("BTC", 50000m);
        var newRate = new Rate("BTC", 55000m);
        
        Assert.True(newRate.HasSignificantChange(oldRate, 5m));
        Assert.Equal(10m, newRate.CalculatePercentageChange(oldRate));
    }
}