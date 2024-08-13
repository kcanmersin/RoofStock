using Core.Data.Entity.EntityBases;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entity.User
{
    public class AppUser : IdentityUser<Guid>, IEntityBase
    {
        public override string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string Email { get; set; }
        public decimal Balance { get; set; } = 0; //USD
        public string ImagePath { get; set; } ="/API/defaultlogo.jpg";


        //for registration
        public bool IsEmailConfirmed { get; set; } = false; 
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationSentAt { get; set; }


        //navigation properties
        public virtual ICollection<StockHolding> StockHoldings { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        //created date
        
  



    }
}
