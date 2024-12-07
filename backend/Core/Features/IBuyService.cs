using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity.User;
using Core.Shared;

namespace Core.Features
{
    public interface IBuyService
    {
        Task<Result> BuyStockAsync(AppUser user, string stockSymbol, int quantity, decimal price, Guid orderProcessId = default);
    }
}