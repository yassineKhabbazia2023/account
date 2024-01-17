using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kpmg.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Paging<Kpmg.Account.Core.Models.Account> GetAccountList(string search, int page, int limit);

        public AccountDetail GetAccountDetail(Guid id);

        public AccountDetail UpdateAccount(Guid id, AccountDetail updatedAccount);

        public List<AccountFavorite> GetAccountFavoriteList(string token);

        public void SetFavorite(Guid id, bool isFavorite, string token);
    }
}
