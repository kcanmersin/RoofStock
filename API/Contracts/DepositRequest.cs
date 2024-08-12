using System.ComponentModel;

namespace Core.Features.Deposit
{
    public class DepositRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid UserId { get; set; }
        
        [DefaultValue(100.00)]
        public decimal Amount { get; set; }
        //Currency
        [DefaultValue("TRY")]
        public string Currency { get; set; }

    }
}
