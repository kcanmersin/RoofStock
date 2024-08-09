using System.ComponentModel;

namespace Core.Features.Deposit
{
    public class DepositRequest
    {
        [DefaultValue("657c6705-0728-4dfc-8019-ea59c42f984f")]
        public Guid UserId { get; set; }
        
        [DefaultValue(100.00)]
        public decimal Amount { get; set; }
        //Currency
        [DefaultValue("TRY")]
        public string Currency { get; set; }

    }
}
