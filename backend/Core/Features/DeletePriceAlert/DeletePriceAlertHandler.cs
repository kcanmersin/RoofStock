using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.Features.DeletePriceAlert
{
    public class DeletePriceAlertHandler : IRequestHandler<DeletePriceAlertCommand, Result>
    {
        private readonly ApplicationDbContext _context;

        public DeletePriceAlertHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeletePriceAlertCommand request, CancellationToken cancellationToken)
        {
            var alert = await _context.StockPriceAlerts
                .FirstOrDefaultAsync(a => a.Id == request.AlertId && a.UserId == request.UserId, cancellationToken);

            if (alert == null)
            {
                return Result.Failure(new Error("AlertNotFound", "Price alert not found."));
            }

            _context.StockPriceAlerts.Remove(alert);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
