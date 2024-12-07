using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.Withdrawal
{
    public class WithdrawalResponse
    {
        public bool IsSuccess { get; set; }
        public decimal NewBalance { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string TargetCurrency { get; set; } = "USD";
        public string Message { get; set; } = string.Empty;
    }
}