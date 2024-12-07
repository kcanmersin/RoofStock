using Core.Shared;
using MediatR;

namespace Core.Features.Deposit
{
    public class DepositCommand : IRequest<Result<DepositResponse>>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } 
    }
}
