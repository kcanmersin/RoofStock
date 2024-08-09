using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.JWT
{
    public interface IJwtService
    {
        string GenerateToken(string email, Guid userId);
    }
}
