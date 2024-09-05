using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.Withdrawal
{
  public class WithdrawalRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid? UserId { get; set; }
        [DefaultValue(100.00)]
        public decimal? AmountInUSD { get; set; }
        [DefaultValue("TRY")]
        public string? TargetCurrency { get; set; } = string.Empty;
    }
}