using Core.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioCommand : IRequest<Result<ShowPortfolioResponse>>
    {
        public Guid UserId { get; set; }
    }
}
