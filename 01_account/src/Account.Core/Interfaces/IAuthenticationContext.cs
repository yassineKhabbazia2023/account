using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAuthenticationContext
    {
        public string BearerToken { get; }
    }
}
