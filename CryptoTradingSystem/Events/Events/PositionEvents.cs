using MediatR;

namespace Events
{
    public record AddPositionCommand : IRequest<Guid>
    {
        public string InstrumentId { get; init; }
        public decimal Quantity { get; init; }
        public decimal InitialRate { get; init; }
        public string Side { get; init; } // "BUY" or "SELL"
    }

    public record ClosePositionCommand : IRequest<bool>
    {
        public Guid PositionId { get; init; }
    }

    public record PositionValueCalculatedEvent : INotification
    {
        public Guid PositionId { get; init; }
        public string InstrumentId { get; init; }
        public decimal Quantity { get; init; }
        public decimal InitialRate { get; init; }
        public decimal CurrentRate { get; init; }
        public string Side { get; init; }
        public decimal ProfitLoss { get; init; }
        public DateTime CalculatedAt { get; init; }
    }
}