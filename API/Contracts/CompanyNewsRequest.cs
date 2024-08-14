using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contracts
{
public class CompanyNewsRequest
{
    [DefaultValue("AAPL")]
    public string Symbol { get; set; }
    [DefaultValue("2021-01-01")]
    public string From { get; set; }
    [DefaultValue("2024-01-01")]
    public string To { get; set; } 

}

}