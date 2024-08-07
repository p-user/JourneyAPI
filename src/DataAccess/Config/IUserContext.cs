using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Config
{
    public interface IUserContext
    {
        string GetUserId();
    }
}
