using MediatR;

namespace Events
{
    public record RateChangedEvent : INotification
    {
        public string InstrumentId { get; init; }
        public decimal OldRate { get; init; }
        public decimal NewRate { get; init; }
        public decimal PercentageChange { get; init; }
        public DateTime Timestamp { get; init; }
    }

    public record FetchRatesCommand : IRequest
    {
        public DateTime TriggeredAt { get; init; } = DateTime.UtcNow;
    }
}