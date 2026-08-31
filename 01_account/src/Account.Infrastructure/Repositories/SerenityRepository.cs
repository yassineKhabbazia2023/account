// <copyright file="SerenityRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class SerenityRepository : ISerenityRepository
{
    // Codes SQL Server de violation d'unicite : contrainte PK/UNIQUE et index unique.
    private const int UniqueConstraintViolation = 2627;
    private const int UniqueIndexViolation = 2601;

    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public SerenityRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<SerenityEligibility> GetSerenityEligibilityAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var hasMadeChoice = await _accountContext.SerenityChoiceEntity
                    .AsNoTracking()
                    .AnyAsync(choice => choice.ContactId == contactId);

            // Un choix exprimé éteint la modal : inutile d'interroger le portefeuille.
            if (hasMadeChoice)
            {
                return new SerenityEligibility { HasMadeChoice = true, CandidateAccountIds = [] };
            }

            // AccountEntity (et non ActiveAccounts) : on conserve volontairement le filtre global,
            // qui écarte les comptes inactifs et de type prospect.
            // ToLower() des deux côtés, comme le reste des repositories : la comparaison ne dépend
            // plus de la collation du provider (CI_AS en base, sensible à la casse en InMemory).
            // Projection sur l'identifiant seul, sans Include : la Gateway n'a besoin que d'ids.
            var candidateAccountIds = await _accountContext.AccountEntity
                    .AsNoTracking()
                    .Where(account => account.RoleEntity.Any(role => role.ContactId == contactId))
                    .Where(account => account.AccountRoutingCode != null
                                   && account.AccountRoutingCode.ToLower() == AccountRoutingCodes.B2B.ToLower())
                    .Where(account => account.AccountElectronicAddressId == null
                                   || account.AccountElectronicAddressId.Trim() == string.Empty)
                    .Select(account => account.AccountId)
                    .ToListAsync();

            return new SerenityEligibility
            {
                HasMadeChoice = false,
                CandidateAccountIds = candidateAccountIds,
            };
        });
    }

    // Aucune politique de retry sur ce chemin, volontairement : la clé primaire est fournie par
    // l'appelant, un rejeu re-exécuterait AddAsync et EF Core lèverait « another instance with the
    // same key value is already being tracked » au lieu du 409. DelegationRequestRepository, la
    // slice la plus proche, ne retente pas non plus ses écritures.
    public async Task CreateSerenityChoiceAsync(int contactId, bool isAccepted)
    {
        var alreadyExists = await _accountContext.SerenityChoiceEntity
                .AsNoTracking()
                .AnyAsync(choice => choice.ContactId == contactId);

        if (alreadyExists)
        {
            throw new ConflictException(
                Errors.SerenityChoiceAlreadyExistsCode,
                string.Format(Errors.SerenityChoiceAlreadyExistsMessage, contactId));
        }

        await _accountContext.SerenityChoiceEntity.AddAsync(new SerenityChoiceEntity
        {
            ContactId = contactId,
            IsAccepted = isAccepted,
            ChoiceDate = DateTime.UtcNow,
        });

        try
        {
            await _accountContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: UniqueConstraintViolation or UniqueIndexViolation })
        {
            // Double soumission concurrente : la clé primaire a tranché, on rend le même 409.
            // Filtré sur la violation d'unicité seule : depuis l'ajout de la FK vers actor.Contact,
            // un contactId inexistant lèverait aussi un DbUpdateException, qui n'est pas un conflit.
            throw new ConflictException(
                Errors.SerenityChoiceAlreadyExistsCode,
                string.Format(Errors.SerenityChoiceAlreadyExistsMessage, contactId));
        }
    }
}
