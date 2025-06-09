using MediatR;
using Events;
using Microsoft.Extensions.Logging;

namespace Messaging
{
    public interface IPositionValuePublisher
    {
        Task PublishAsync(PositionValueCalculatedEvent positionValue);
    }

    public class ConsolePositionValuePublisher : IPositionValuePublisher
    {
        private readonly ILogger<ConsolePositionValuePublisher> _logger;

        public ConsolePositionValuePublisher(ILogger<ConsolePositionValuePublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(PositionValueCalculatedEvent positionValue)
        {
            // In a real implementation, this would publish to a message queue
            // For this test, we'll output to console/logs
            
            _logger.LogInformation(
                "📊 Position Value Calculated:\n" +
                "   Position ID: {PositionId}\n" +
                "   Instrument: {InstrumentId}\n" +
                "   Quantity: {Quantity}\n" +
                "   Initial Rate: {InitialRate:C}\n" +
                "   Current Rate: {CurrentRate:C}\n" +
                "   Side: {Side}\n" +
                "   💰 Profit/Loss: {ProfitLoss:C}\n" +
                "   Calculated At: {CalculatedAt}",
                positionValue.PositionId,
                positionValue.InstrumentId,
                positionValue.Quantity,
                positionValue.InitialRate,
                positionValue.CurrentRate,
                positionValue.Side,
                positionValue.ProfitLoss,
                positionValue.CalculatedAt
            );

            return Task.CompletedTask;
        }
    }

    public class PositionValueHandler : INotificationHandler<PositionValueCalculatedEvent>
    {
        private readonly IPositionValuePublisher _publisher;

        public PositionValueHandler(IPositionValuePublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Handle(PositionValueCalculatedEvent notification, CancellationToken cancellationToken)
        {
            await _publisher.PublishAsync(notification);
        }
    }
}