using System;

namespace API.Contracts
{
    public class DeletePriceAlertRequest
    {
        public Guid UserId { get; set; }
        public Guid AlertId { get; set; }
    }
}
