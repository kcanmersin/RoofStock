using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.Withdrawal
{
  public class WithdrawalRequest
    {
        [DefaultValue("657c6705-0728-4dfc-8019-ea59c42f984f")]
        public Guid UserId { get; set; }
        [DefaultValue(100.00)]
        public decimal AmountInUSD { get; set; }
        [DefaultValue("TRY")]
        public string TargetCurrency { get; set; } = string.Empty;
    }
}