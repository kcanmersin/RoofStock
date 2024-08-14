using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contracts
{
public class MarketNewsRequest
{
    [DefaultValue("general")]
    public string Category { get; set; }
    [DefaultValue(0)]
    public int? MinId { get; set; } 

}

}