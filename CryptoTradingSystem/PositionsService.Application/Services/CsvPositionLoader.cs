using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using PositionsService.Domain.Entities;
using PositionsService.Infra.Persistence;

namespace PositionsService.Application.Services
{
    public interface ICsvPositionLoader
    {
        Task LoadPositionsFromCsvAsync(string filePath);
    }

    public class CsvPositionLoader : ICsvPositionLoader
    {
        private readonly PositionsDbContext _context;

        public CsvPositionLoader(PositionsDbContext context)
        {
            _context = context;
        }

        public async Task LoadPositionsFromCsvAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<CsvPosition>();
            
            foreach (var record in records)
            {
                var position = new Position(
                    record.InstrumentId,
                    record.Quantity,
                    record.InitialRate,
                    record.Side
                );
                
                _context.Positions.Add(position);
            }
            
            await _context.SaveChangesAsync();
        }
    }

    public class CsvPosition
    {
        public string InstrumentId { get; set; }
        public decimal Quantity { get; set; }
        public decimal InitialRate { get; set; }
        public string Side { get; set; }
    }
}