using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contracts
{
public class CancelOrderRequest
{
    public Guid? OrderId { get; set; }
    [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
    public Guid? UserId { get; set; }
}

}