using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Features.GiveOrder
{
public class OrderResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public Guid OrderId { get; set; }
}

}