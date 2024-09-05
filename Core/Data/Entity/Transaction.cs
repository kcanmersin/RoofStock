using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Entity.EntityBases;
using Core.Data.Entity.User;
namespace Core.Data.Entity
{
    /*
Table transactions {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  amount decimal
  type enum // Example values: 'deposit', 'withdrawal'
  description text
  stockholdingitem_id uuid [ref: > stockholdingitem.id] //nullable
 
  //ref ekle stockholdinge
}
     */
    public class Transaction : EntityBase
    {
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }

        public decimal Amount { get; set; }
        //buradaki tip userın uygulamadan para çekip para yatırması. satın al sat işlemindeki para kullanıcının hesabındaki balanca eklenip düşecek

        public TransActionType Type { get; set; }

        public string Description { get; set; }

        //stockholding item might be null
        public Guid? StockHoldingItemId { get; set; }
        public StockHoldingItem StockHoldingItem { get; set; }

    }

    //hisse sattığımızda balaance artacağı  için transaction postive olacak
    //hisse aldığımızda balance azalacağı için transaction negative olacak
    //deposit yaptığımızda balance artacağı için transaction postive olacak
    //withdrawal yaptığımızda balance azalacağı için transaction negative olacak
    public enum TransActionType
    {
        Positive,
        Negative
    }
  
}
