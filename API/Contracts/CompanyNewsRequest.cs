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

        [DefaultValue(1)]
        public int Page { get; set; } = 1;
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

    }

}