using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid UserId { get; set; }
    }
}
