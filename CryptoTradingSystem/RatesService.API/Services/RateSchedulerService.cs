using MediatR;
using Events;

namespace RatesService.API.Services
{
    public class RateSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RateSchedulerService> _logger;
        private readonly int _intervalMinutes;

        public RateSchedulerService(
            IServiceProvider serviceProvider, 
            ILogger<RateSchedulerService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _intervalMinutes = configuration.GetValue<int>("Scheduler:IntervalMinutes", 5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rate Scheduler Service started. Will fetch rates every {interval} minutes", _intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        
                        _logger.LogInformation("Triggering rate fetch at {time}", DateTime.UtcNow);
                        await mediator.Send(new FetchRatesCommand(), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching rates");
                }

                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }
        }
    }
}