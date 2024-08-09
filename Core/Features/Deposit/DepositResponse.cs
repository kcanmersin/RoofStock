namespace Core.Features.Deposit
{
    public class DepositResponse
    {
        public bool IsSuccess { get; set; }
        public decimal NewBalance { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal USDConvertedAmount { get; set; } 
    }
}
