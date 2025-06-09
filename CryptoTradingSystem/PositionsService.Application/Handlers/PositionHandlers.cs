using MediatR;
using Microsoft.EntityFrameworkCore;
using Events;
using PositionsService.Domain.Entities;
using PositionsService.Infra.Persistence;

namespace PositionsService.Application.Handlers
{
    public class AddPositionHandler : IRequestHandler<AddPositionCommand, Guid>
    {
        private readonly PositionsDbContext _context;

        public AddPositionHandler(PositionsDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(AddPositionCommand request, CancellationToken cancellationToken)
        {
            var position = new Position(
                request.InstrumentId,
                request.Quantity,
                request.InitialRate,
                request.Side
            );

            _context.Positions.Add(position);
            await _context.SaveChangesAsync(cancellationToken);

            return position.Id;
        }
    }

    public class ClosePositionHandler : IRequestHandler<ClosePositionCommand, bool>
    {
        private readonly PositionsDbContext _context;

        public ClosePositionHandler(PositionsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        {
            var position = await _context.Positions.FindAsync(new object[] { request.PositionId }, cancellationToken);
            
            if (position == null)
                return false;

            position.Close();
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }

    public class RateChangedHandler : INotificationHandler<RateChangedEvent>
    {
        private readonly PositionsDbContext _context;
        private readonly IMediator _mediator;

        public RateChangedHandler(PositionsDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task Handle(RateChangedEvent notification, CancellationToken cancellationToken)
        {
            var openPositions = await _context.Positions
                .Where(p => p.InstrumentId == notification.InstrumentId && p.Status == PositionStatus.Open)
                .ToListAsync(cancellationToken);

            foreach (var position in openPositions)
            {
                var profitLoss = position.CalculateProfitLoss(notification.NewRate);

                await _mediator.Publish(new PositionValueCalculatedEvent
                {
                    PositionId = position.Id,
                    InstrumentId = position.InstrumentId,
                    Quantity = position.Quantity,
                    InitialRate = position.InitialRate,
                    CurrentRate = notification.NewRate,
                    Side = position.Side.ToString(),
                    ProfitLoss = profitLoss,
                    CalculatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }
    }
}