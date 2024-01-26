using Kpmg.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Task<Paging<Kpmg.Account.Core.Models.Account>> GetAccountsAsync(string? search, int page, int limit);

        public AccountDetail GetAccountDetailAsync(Guid id);

        public AccountDetail UpdateAccountAsync(Guid id, AccountDetail accountDetail);

        public IReadOnlyCollection<AccountFavorite> GetAccountFavoritesAsync(Guid contactId);

        public void SetFavoriteAsync(Guid accountId, Guid contactId, bool isFavorite);
    }
}
