namespace PositionsService.Domain.Entities
{
    public class Position
    {
        public Guid Id { get; private set; }
        public string InstrumentId { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal InitialRate { get; private set; }
        public PositionSide Side { get; private set; }
        public PositionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }

        protected Position() { } // EF Core

        public Position(string instrumentId, decimal quantity, decimal initialRate, string side)
        {
            Id = Guid.NewGuid();
            InstrumentId = instrumentId ?? throw new ArgumentNullException(nameof(instrumentId));
            Quantity = quantity;
            InitialRate = initialRate;
            Side = Enum.Parse<PositionSide>(side, true);
            Status = PositionStatus.Open;
            CreatedAt = DateTime.UtcNow;
        }

        public decimal CalculateProfitLoss(decimal currentRate)
        {
            var sideMultiplier = Side == PositionSide.Buy ? 1 : -1;
            return Quantity * (currentRate - InitialRate) * sideMultiplier;
        }

        public void Close()
        {
            if (Status == PositionStatus.Closed)
                throw new InvalidOperationException("Position is already closed");

            Status = PositionStatus.Closed;
            ClosedAt = DateTime.UtcNow;
        }
    }

    public enum PositionSide
    {
        Buy,
        Sell
    }

    public enum PositionStatus
    {
        Open,
        Closed
    }
}