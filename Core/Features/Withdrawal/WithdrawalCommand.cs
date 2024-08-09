using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Shared;
using MediatR;

namespace Core.Features.Withdrawal
{
    public class WithdrawalCommand : IRequest<Result<WithdrawalResponse>>
    {
        public Guid UserId { get; set; }
        public decimal AmountInUSD { get; set; }
        public string TargetCurrency { get; set; } = string.Empty;
    }
}