// <copyright file="MapAccountDbToAccountModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapAccountDbToAccountModel
{
    public static Paging<AccountModel> MapToPaginAccounts(
        this ICollection<AccountEntity> source,
        int? contactId,
        int pageNumber,
        int totalRows,
        int totalPageCalcul)
    {
        return
            new Paging<AccountModel>()
            {
                Items = source.MapToAccounts(contactId),
                CurrentPage = pageNumber,
                TotalItems = totalRows,
                TotalPage = totalPageCalcul
            };
    }

    public static Paging<Contact> MapToPagingContact(
        this IEnumerable<Contact> source,
        int pageNumber,
        int totalRows,
        int totalPageCalcul)
    {
        return
            new Paging<Contact>()
            {
                Items = source,
                CurrentPage = pageNumber,
                TotalItems = totalRows,
                TotalPage = totalPageCalcul
            };
    }

    public static IEnumerable<AccountModel> MapToAccounts(this ICollection<AccountEntity> source, int? contactId)
    {
        return source?.Select(a => a.MapToAccount(contactId)!) ?? [];
    }

    public static AccountModel? MapToAccount(this AccountEntity source, int? contactId)
    {
        if (source == null)
        {
            return null;
        }

        var signatory = source.RoleEntity?.FirstOrDefault(r => r.IsSignatory == true && r.Contact.Type != ContactType.Collaborator.ToString());
        var currentContact = source.RoleEntity?.FirstOrDefault(r => r.ContactId == contactId);

        return
            new AccountModel
            {
                AccountId = source.AccountId,
                AccountGlobalUniqueId = source.AccountGlobalUniqueId,
                AccountNumber = source.AccountNumber,
                LegalName = source.LegalName,
                IsFavorite = currentContact?.IsFavorite,
                IsCustomerRelation = currentContact?.IsCustomerRelation,
                OfficeId = source.OfficeId,
                Office = source.Office?.MapToOffice(),
                Address = source.MapToAddressDelivery(),
                Signatory = signatory?.Contact.MapToContact(null!),
                Deployment = source.MapToDeployment(),
                Hub = source.MapToHub()
            };
    }

    public static AccountModel? MapToAccountSummary(this AccountEntity source, int contactId)
    {
        var isSignatory = source.RoleEntity.Any(role => role.ContactId == contactId && role.IsSignatory == true);

        return source == null ? null : new AccountModel
        {
            AccountId = source.AccountId,
            AccountGlobalUniqueId = source.AccountGlobalUniqueId,
            AccountNumber = source.AccountNumber,
            LegalName = source.LegalName,
            OfficeId = source.OfficeId,
            Office = source.Office?.MapToOffice(),
            MissionType = source.MissionType,
            Address = source.MapToAddressDelivery(),
            Deployment = source.MapToDeployment(),
            IsClarityVisible = source.MapToIsClarityVisible(),
            IsSignatory = isSignatory
        };
    }

    public static Office? MapToOffice(this OfficeEntity source)
    {
        return source == null ? null : new Office
        {
            OfficeId = source.OfficeId,
            Name = source.Name,
            Address = source.Address.MapToAddress(),
            PhoneNumber = source.PhoneNumber,
            AddressId = source.AddressId,
        };
    }

    public static bool MapToIsClarityVisible(this AccountEntity? source)
    {
        if (source == null || source.OfferEligibilityEntity == null)
        {
            return false;
        }

        var offerEligibility = source.OfferEligibilityEntity;

        return offerEligibility.IsEligible && (!offerEligibility.ApprovedDate.HasValue || offerEligibility.ApprovedDate == DateTime.MinValue);
    }

    public static void MapToActivatedOfferEligibility(this OfferEligibilityEntity? existingEntity, string approvedBy)
    {
        if (existingEntity != null)
        {
            existingEntity.IsEligible = false;
            existingEntity.ApprovedBy = approvedBy;
            existingEntity.ApprovedDate = DateTime.UtcNow;
        }
    }

    public static OfferEligibility? MapToOfferEligibility(this OfferEligibilityEntity? offerEligibility)
    {
        return offerEligibility == null ? null : new OfferEligibility
        {
            AccountId = offerEligibility.AccountId,
            OfferName = offerEligibility.OfferName,
            ApprovedBy = offerEligibility.ApprovedBy,
            ApprovedDate = offerEligibility.ApprovedDate,
            IsEligible = offerEligibility.IsEligible,
        };
    }

    public static OfficeEntity? MapToOffice(this Office source)
    {
        return source == null ? null : new OfficeEntity
        {
            OfficeId = source.OfficeId,
            Name = source.Name,
            Address = source!.Address!.MapToAddressEntity(),
            PhoneNumber = source.PhoneNumber,
            AddressId = source.AddressId,
        };
    }

    public static AccountDetail? MapToAccountDetail(this AccountEntity source)
    {
        return source == null ? null :
        new AccountDetail
        {
            AccountId = source.AccountId,
            AccountGlobalUniqueId = source.AccountGlobalUniqueId,
            AccountNumber = source.AccountNumber,
            IconName = source.IconName,
            IsActive = source.IsActive,
            Email = source.Email,
            EmployeeCount = source.StaffSize,
            CommercialName = source.CommercialName,
            Accounting = source.MapToAccounting(),
            Legal = source.MapToLegal(),
            Vat = source.MapToVat(),
            Address = source.MapToAddress(),
            Phone = source.MapToPhone(),
            Hub = source.MapToHub(),
            Deployment = source.MapToDeployment(),
            CreatedBy = source.CreatedBy,
            ModifiedBy = source.ModifiedBy,
            MissionType = source.MissionType,
            OfficeId = source.OfficeId,
            Office = source.Office?.MapToOffice(),
            Turnover = source.Turnover
        };
    }

    public static Contact? MapToContact(this ContactEntity tContact, int? accountId)
    {
        return tContact == null ? null : new Contact
        {
            ContactId = tContact.ContactId,
            ContactGlobalUniqueId = tContact.ContactGlobalUniqueId,
            Email = tContact.Email,
            FirstName = tContact.FirstName,
            LastName = tContact.LastName,
            LandPhone = tContact.LandPhone,
            MobilePhone = tContact.MobilePhone,
            Status = tContact.Status,
            PersonaName = tContact.PersonaName,
            Office = tContact.Office,
            CreationDate = tContact.CreationDate,
            Type = tContact.Type,
            IsActive = tContact.IsActive,
            IsCustomerRelation = tContact.RoleEntity.FirstOrDefault(r => r.AccountId == accountId)?.IsCustomerRelation,
            ActionLevel = tContact.RoleEntity.FirstOrDefault(r => r.AccountId == accountId)?.ActionLevel ?? 0,
            Labels = tContact.RoleLabelEntityContact.MapToLabels(accountId ?? 0),
        };
    }

    private static IEnumerable<Label> MapToLabels(this IEnumerable<RoleLabelEntity> source, int accountId)
    {
        return source?.Where(l => l.AccountId == accountId).Select(l => l.Label.Map() !) ?? Enumerable.Empty<Label>();
    }

    public static Deployment? MapToDeployment(this AccountEntity tAccount)
    {
        if (tAccount?.DeploymentEntity == null)
        {
            return new Deployment();
        }

        var deployment = tAccount.DeploymentEntity;

        return new Deployment
        {
            Status = deployment.Status,
            DeploymentId = deployment.DeploymentId,
            DeploymentDate = deployment.DeploymentDate,
        };
    }

    public static void UpdateDeploymentToEntity(this Deployment deployment, AccountEntity tAccount)
    {
        var deploymentStatus = deployment.Status;

        tAccount.DeploymentEntity = new DeploymentEntity
        {
            Status = deploymentStatus,
            DeploymentDate = DateTime.UtcNow,
        };
    }

    private static IEnumerable<Phone>? MapToPhone(this AccountEntity tAccount)
    {
        return tAccount.PhoneEntity == null ? Array.Empty<Phone>() :
            tAccount.PhoneEntity.Select(phone => new Phone
            {
                Type = phone.Type,
                PhoneId = phone.PhoneId,
                PhoneNumber = phone.PhoneNumber,
            });
    }

    private static Address MapToAddressDelivery(this AccountEntity tAccount)
    {
        if (tAccount == null)
        {
            return new Address();
        }

        var address = tAccount.AddressEntity.FirstOrDefault(address => address.AddressType == AddressType.Delivery.ToString());

        return MapToAddress(address);
    }

    private static Address MapToAddress(this AddressEntity? address) => address == null ? new Address() : new Address
    {
        AddressId = address!.AddressId,
        Country = address.Country,
        City = address.City,
        State = address.State,
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        AddressLine3 = address.AddressLine3,
        ZipCode = address.ZipCode,
        AddressType = address.AddressType,
        Longitude = address.Longitude,
        Latitude = address.Latitude
    };

    private static AddressEntity MapToAddressEntity(this Address? address) => address == null ? new AddressEntity() : new AddressEntity
    {
        AddressId = address!.AddressId,
        Country = address.Country,
        City = address.City,
        State = address.State,
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        AddressLine3 = address.AddressLine3,
        ZipCode = address.ZipCode,
        AddressType = address.AddressType,
        Longitude = address.Longitude,
        Latitude = address.Latitude
    };

    private static IEnumerable<Address>? MapToAddress(this AccountEntity tAccount)
    {
        return tAccount.AddressEntity == null ? Array.Empty<Address>() :
            tAccount.AddressEntity.Select(address => new Address
            {
                AddressId = address.AddressId,
                Country = address.Country,
                City = address.City,
                State = address.State,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                ZipCode = address.ZipCode,
                AddressType = address.AddressType,
                Longitude = address.Longitude,
                Latitude = address.Latitude
            });
    }

    private static Hub MapToHub(this AccountEntity tAccount)
    {
        return new Hub()
        {
            HubId = tAccount.Hub?.HubId,
            HubName = tAccount.Hub?.HubName,
        };
    }

    private static Vat MapToVat(this AccountEntity tAccount)
    {
        return new Vat()
        {
            System = tAccount.Vat,
            Intra = tAccount.Vatintra,
            Type = tAccount.Vattype,
        };
    }

    private static Accounting MapToAccounting(this AccountEntity tAccount)
    {
        return new Accounting
        {
            FiscalExerciseStartDate = tAccount.FiscalExerciseStartDate,
            FiscalExerciseDuration = tAccount.FiscalExerciseDuration,
            AccountingType = tAccount.AccountingMethod,
            FiscalSystem = tAccount.FiscalSystem,
            TaxationSystem = tAccount.TaxationSystem,
            ActivityType = tAccount.ActivityType,
            ActivityDescription = tAccount.ActivityDescription,
        };
    }

    private static Legal MapToLegal(this AccountEntity tAccount)
    {
        var siren = !string.IsNullOrWhiteSpace(tAccount?.Siret) && tAccount.Siret.Length >= 9
            ? tAccount.Siret.Substring(0, 9)
            : null;

        return new Legal
        {
            LegalName = tAccount.LegalName,
            Siren = siren,
            Siret = tAccount.Siret,
            LegalForm = tAccount.LegalForm,
            LegalFormCode = tAccount.LegalFormCode,
            StaffSizeRange = tAccount.StaffSizeRange,
            Naf = tAccount.MapToNaf(),
            CreationDate = tAccount.CreationDate
        };
    }

    private static List<Naf> MapToNaf(this AccountEntity tAccount)
    {
        if (tAccount.Naf == null)
        {
            return [];
        }

        var naf = new Naf
        {
            NafId = tAccount.Naf.NafId,
            NafCode = tAccount.Naf.NafCode,
            NafLabel = tAccount.Naf.NafLabel
        };

        return [naf];
    }

    public static Naf? MapToNaf(this NafEntity nafEntity)
    {
        return nafEntity == null ? null : new Naf
        {
            NafId = nafEntity.NafId,
            NafCode = nafEntity.NafCode,
            NafLabel = nafEntity.NafLabel
        };
    }

    public static Statistics MapToStatistics(Dictionary<int, int> countByAccountStatus, Dictionary<string, int> countByContactStatus)
    {
        countByAccountStatus = countByAccountStatus == null || countByAccountStatus.Count == 0 ? [] : countByAccountStatus;
        countByContactStatus = countByContactStatus == null || countByContactStatus.Count == 0 ? [] : countByContactStatus;

        return InitStatistic(countByAccountStatus, countByContactStatus);
    }

    public static Statistics InitStatistic(Dictionary<int, int> countByAccountStatus, Dictionary<string, int> countByContactStatus)
    {
        return new Statistics
        {
            AccountToDeploy = countByAccountStatus?.TryGetValue(1, out var toDeploy) == true ? toDeploy : 0,
            AccountInProgress = countByAccountStatus?.TryGetValue(2, out var inProgress) == true ? inProgress : 0,
            AccountConnected = countByAccountStatus?.TryGetValue(3, out var connected) == true ? connected : 0,
            AccountRevoked = countByAccountStatus?.TryGetValue(4, out var revoked) == true ? revoked : 0,
            ContactConnected = countByContactStatus?.TryGetValue(ContactStatus.Connected.ToString(), out var contactConnected) == true ? contactConnected : 0,
            ContactDeclared = countByContactStatus?.TryGetValue(ContactStatus.Declared.ToString(), out var contactDeclared) == true ? contactDeclared : 0,
            ContactInvited = countByContactStatus?.TryGetValue(ContactStatus.Invited.ToString(), out var contactInvited) == true ? contactInvited : 0
        };
    }

    public static void MapToUpdatedAccount(this AccountEntity existingAccount, AccountDetail accountDetail)
    {
        if (accountDetail.Legal != null)
        {
            existingAccount.StaffSizeRange = accountDetail.Legal.StaffSizeRange;
        }

        if (accountDetail.Accounting != null)
        {
            existingAccount.FiscalExerciseStartDate = accountDetail.Accounting.FiscalExerciseStartDate;
            existingAccount.FiscalExerciseDuration = accountDetail.Accounting.FiscalExerciseDuration;
            existingAccount.AccountingMethod = accountDetail.Accounting.AccountingType;
            existingAccount.FiscalSystem = accountDetail.Accounting.FiscalSystem;
            existingAccount.TaxationSystem = accountDetail.Accounting.TaxationSystem;
            existingAccount.ActivityType = accountDetail.Accounting.ActivityType;
            existingAccount.ActivityDescription = accountDetail.Accounting.ActivityDescription;
        }

        if (accountDetail.Deployment != null)
        {
            if (existingAccount.DeploymentEntity == null)
            {
                existingAccount.DeploymentEntity = new DeploymentEntity
                {
                    AccountId = existingAccount.AccountId,
                };
            }

            existingAccount.DeploymentEntity.Status = accountDetail.Deployment.Status;
            existingAccount.DeploymentEntity.DeploymentDate = accountDetail.Deployment.DeploymentDate ?? DateTime.UtcNow;
        }

        existingAccount.OfficeId = accountDetail.OfficeId;
        existingAccount.Office = accountDetail.Office?.MapToOffice();
        existingAccount.HubId = accountDetail.Hub?.HubId;
        existingAccount.Vat = accountDetail.Vat?.System;
        existingAccount.Vattype = accountDetail.Vat?.Type;
        existingAccount.Turnover = accountDetail.Turnover;
        existingAccount.MissionType = accountDetail.MissionType;
    }
}
