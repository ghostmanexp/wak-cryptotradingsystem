using MediatR;
using Microsoft.EntityFrameworkCore;
using Events;
using RatesService.Application.Services;
using RatesService.Domain.Entities;
using RatesService.Infra.Persistence;

namespace RatesService.Application.Handlers
{
    public class FetchRatesHandler : IRequestHandler<FetchRatesCommand>
    {
        private readonly ICoinMarketCapClient _client;
        private readonly RatesDbContext _context;
        private readonly IMediator _mediator;
        private const decimal THRESHOLD_PERCENTAGE = 5.0m;

        public FetchRatesHandler(ICoinMarketCapClient client, RatesDbContext context, IMediator mediator)
        {
            _client = client;
            _context = context;
            _mediator = mediator;
        }

        public async Task Handle(FetchRatesCommand request, CancellationToken cancellationToken)
        {
            var response = await _client.GetLatestListingsAsync();
            var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

            foreach (var crypto in response.Data)
            {
                var symbol = crypto.Symbol;
                var currentValue = crypto.Quote.USD.Price;

                // Create new rate
                var newRate = new Rate(symbol, currentValue);
                _context.Rates.Add(newRate);

                // Get oldest rate in last 24 hours
                var oldestRate = await _context.Rates
                    .Where(r => r.Symbol == symbol && r.Timestamp >= twentyFourHoursAgo)
                    .OrderBy(r => r.Timestamp)
                    .FirstOrDefaultAsync(cancellationToken);

                if (oldestRate != null && newRate.HasSignificantChange(oldestRate, THRESHOLD_PERCENTAGE))
                {
                    var percentageChange = newRate.CalculatePercentageChange(oldestRate);
                    
                    // Publish rate changed event
                    await _mediator.Publish(new RateChangedEvent
                    {
                        InstrumentId = $"{symbol}/USD",
                        OldRate = oldestRate.Value,
                        NewRate = currentValue,
                        PercentageChange = percentageChange,
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}