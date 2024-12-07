using System;
using Core.Shared;
using MediatR;

namespace Core.Features.DeletePriceAlert
{
    public class DeletePriceAlertCommand : IRequest<Result>
    {
        public Guid UserId { get; set; }
        public Guid AlertId { get; set; }
    }
}
