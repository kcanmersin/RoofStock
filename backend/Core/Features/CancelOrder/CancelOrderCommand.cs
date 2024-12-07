using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Shared;
using MediatR;

namespace Core.Features.CancelOrder
{
    public class CancelOrderCommand : IRequest<Result<CancelOrderResponse>>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }

}