namespace RatesService.Domain.Entities
{
    public class Rate
    {
        public Guid Id { get; private set; }
        public string Symbol { get; private set; }
        public decimal Value { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected Rate() { } // EF Core

        public Rate(string symbol, decimal value)
        {
            Id = Guid.NewGuid();
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Value = value;
            Timestamp = DateTime.UtcNow;
        }

        public bool HasSignificantChange(Rate? previousRate, decimal thresholdPercentage = 5.0m)
        {
            if (previousRate == null || previousRate.Symbol != Symbol)
                return false;

            var percentageChange = CalculatePercentageChange(previousRate.Value, Value);
            return Math.Abs(percentageChange) > thresholdPercentage;
        }

        public decimal CalculatePercentageChange(Rate previousRate)
        {
            if (previousRate == null || previousRate.Symbol != Symbol)
                throw new InvalidOperationException("Cannot calculate change with null or different symbol rate");

            return CalculatePercentageChange(previousRate.Value, Value);
        }

        private static decimal CalculatePercentageChange(decimal oldValue, decimal newValue)
        {
            if (oldValue == 0) return 0;
            return ((newValue - oldValue) / oldValue) * 100;
        }
    }
}