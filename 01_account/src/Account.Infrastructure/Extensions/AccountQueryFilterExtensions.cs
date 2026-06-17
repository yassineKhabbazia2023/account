// <copyright file="AccountQueryFilterExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Extensions
{
    /// <summary>
    /// Filtres composables appliqués à la recherche d'entités morales.
    /// Chaque filtre prend la requête déjà filtrée et renvoie la requête affinée,
    /// garantissant un cumul (ET) par construction.
    /// </summary>
    public static class AccountQueryFilterExtensions
    {
        public static IQueryable<AccountEntity> ApplySearch(this IQueryable<AccountEntity> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return query;
            }

            var term = search.Trim();

            // Filtrer par LegalName, AccountNumber, ou par un rôle signataire dont le Contact correspond à la recherche
            return query.Where(a =>
                a.LegalName.Contains(term) ||
                a.AccountNumber.Contains(term) ||
                a.RoleEntity.Any(r =>
                    r.IsSignatory == true &&
                    ((r.Contact.FirstName + " " + r.Contact.LastName).Contains(term)
                        || r.Contact.Email.Contains(term))));
        }

        public static IQueryable<AccountEntity> ApplyDeploymentStatus(this IQueryable<AccountEntity> query, int? deploymentStatus)
        {
            if (!deploymentStatus.HasValue)
            {
                return query;
            }

            return query.Where(a => a.DeploymentEntity.Status == deploymentStatus.Value);
        }

        public static IQueryable<AccountEntity> ApplyMissionType(this IQueryable<AccountEntity> query, string? missionType)
        {
            if (string.IsNullOrWhiteSpace(missionType))
            {
                return query;
            }

            return query.Where(a => a.MissionType == missionType);
        }
    }
}
