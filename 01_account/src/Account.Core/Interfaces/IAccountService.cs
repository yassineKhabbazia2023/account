using Kpmg.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Task<Paging<Kpmg.Account.Core.Models.Account>> GetAccountsAsync(string? search, int page, int limit, int contactId);

        public Task<AccountDetail> GetAccountDetailAsync(int id);

        public Task<AccountDetail> UpdateAccountAsync(Guid id, AccountDetail accountDetail);

        public IReadOnlyCollection<AccountFavorite> GetAccountFavoritesAsync(int contactId);

        public void SetFavoriteAsync(int accountId, int contactId, bool isFavorite);
    }
}
