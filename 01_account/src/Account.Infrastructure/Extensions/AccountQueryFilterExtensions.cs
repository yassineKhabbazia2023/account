// <copyright file="AccountQueryFilterExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Utils;

namespace Pulse.Account.Infrastructure.Extensions;

/// <summary>
/// Filtres composables appliqués à la recherche d'entités morales.
/// Chaque filtre prend la requête déjà filtrée et renvoie la requête affinée,
/// garantissant un cumul (ET) par construction.
/// </summary>
public static class AccountQueryFilterExtensions
{
    public static IQueryable<AccountRolePair> ApplyFavorite(this IQueryable<AccountRolePair> query, bool? isFavoriteFilter)
    {
        if (isFavoriteFilter != true)
        {
            return query;
        }

        return query.Where(x => x.Role.IsFavorite == true);
    }

    public static IQueryable<AccountRolePair> ApplyCustomerRelation(this IQueryable<AccountRolePair> query, bool? isCustomerRelationFilter)
    {
        if (isCustomerRelationFilter != true)
        {
            return query;
        }

        return query.Where(x => x.Role.IsCustomerRelation == true);
    }

    public static IQueryable<AccountRolePair> ApplyLastActivityRange(this IQueryable<AccountRolePair> query, DateTime? from, DateTime? to)
    {
        if (from != null)
        {
            query = query.Where(x => x.Role.LastActivityDate >= from);
        }

        if (to != null)
        {
            query = query.Where(x => x.Role.LastActivityDate <= to);
        }

        return query;
    }

    public static IQueryable<AccountRolePair> ApplySearch(this IQueryable<AccountRolePair> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim();

        // Filtrer par LegalName, AccountNumber, ou par un rôle signataire dont le Contact correspond à la recherche
        return query.Where(x =>
            x.Account.LegalName.Contains(term) ||
            x.Account.AccountNumber.Contains(term) ||
            x.Account.RoleEntity.Any(r =>
                r.IsSignatory == true &&
                ((r.Contact.FirstName + " " + r.Contact.LastName).Contains(term)
                    || r.Contact.Email.Contains(term))));
    }

    public static IQueryable<AccountRolePair> ApplyDeploymentStatus(this IQueryable<AccountRolePair> query, ICollection<int>? deploymentStatuses)
    {
        if (deploymentStatuses == null || deploymentStatuses.Count == 0)
        {
            return query;
        }

        return query.Where(x => deploymentStatuses.Contains(x.Account.DeploymentEntity.Status));
    }

    public static IQueryable<AccountRolePair> ApplyMissionType(this IQueryable<AccountRolePair> query, ICollection<string>? missionTypes)
    {
        if (missionTypes == null || missionTypes.Count == 0)
        {
            return query;
        }

        // MissionType = None correspond aux Accounts sans mission renseignée (MissionType null ou vide)
        var includeNone = missionTypes.Contains(MissionType.None.ToString());
        var concreteTypes = missionTypes.Where(m => m != MissionType.None.ToString()).ToList();

        return query.Where(x =>
            concreteTypes.Contains(x.Account.MissionType) ||
            (includeNone && string.IsNullOrEmpty(x.Account.MissionType)));
    }
}
