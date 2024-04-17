// <copyright file="MapAccountDbToAccountModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapAccountDbToAccountModel
    {
        public static Paging<Core.Models.Account> MapToPaginAccounts(
            this ICollection<AccountEntity> source,
            int contactId,
            int pageNumber,
            int totalRows,
            int totalPageCalcul)
        {
            return
             new Paging<Core.Models.Account>()
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

        public static IEnumerable<Core.Models.Account> MapToAccounts(this ICollection<AccountEntity> source, int contactId)
        {
            return source?.Select(a => a.MapToAccount(contactId) !) ?? Enumerable.Empty<Core.Models.Account>();
        }

        public static Core.Models.Account? MapToAccount(this AccountEntity source, int contactId)
        {
            if (source == null)
            {
                return null;
            }

            var signatory = source.RoleEntity?.FirstOrDefault(r => r.IsSignatory == true);
            var currentContact = source.RoleEntity?.FirstOrDefault(r => r.ContactId == contactId);

            return
                new Core.Models.Account
                {
                    AccountId = source.AccountId,
                    AccountGlobalUniqueId = source.AccountGlobalUniqueId,
                    AccountNumber = source.AccountNumber,
                    LegalName = source.LegalName,
                    IsFavorite = currentContact?.IsFavorite,
                    Address = source.MapToAddressDelivery(),
                    Signatory = signatory?.Contact.MapToContact(),
                    Deployment = source.MapToDeployment(),
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
                Deployment = source.MapToDeployment()
            };
        }

        public static Contact? MapToContact(this ContactEntity tContact)
        {
            return tContact == null ? null : new Contact
            {
                ContactId = tContact.ContactId,
                ContactGlobalUniqueId = tContact.ContactGlobalUniqueId,
                Email = tContact.Email,
                FirstName = tContact.FirstName,
                LastName = tContact.LastName,
                Status = tContact.Status,
                PersonaName = tContact.PersonaName,
                Office = tContact.Office,
                CreationDate = tContact.CreationDate,
            };
        }

        private static Deployment? MapToDeployment(this AccountEntity tAccount)
        {
            if (tAccount.DeploymentEntity == null)
            {
                return new Deployment();
            }

            var deploiment = tAccount.DeploymentEntity.Select(deployment => new Deployment
            {
                DeploymentId = deployment.DeploymentId,
                DeploymentDate = deployment.DeploymentDate,
                Status = deployment.Status,
            }).FirstOrDefault();
            return deploiment ?? new Deployment();
        }

        private static IEnumerable<Phone>? MapToPhone(this AccountEntity tAccount)
        {
            return tAccount.PhoneEntity == null ? Array.Empty<Phone>() :
                tAccount.PhoneEntity.Select(phone => new Phone
                {
                    PhoneId = phone.PhoneId,
                    PhoneNumber = phone.PhoneNumber,
                    Type = phone.Type,
                });
        }

        private static Address MapToAddressDelivery(this AccountEntity tAccount)
        {
            if (tAccount == null)
            {
                return new Address();
            }

            var address = tAccount.AddressEntity.FirstOrDefault(address => address.AddressType == AddressType.delivery.ToString());

            return address == null ? new Address() : new Address
            {
                AddressId = address!.AddressId,
                Country = address.Country,
                City = address.City,
                State = address.State,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                ZipCode = address.ZipCode,
                AddressType = address.AddressType
            };
        }

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
                    AddressType = address.AddressType
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
            return new Legal
            {
                LegalName = tAccount.LegalName,
                Siren = tAccount.Isin,
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
                return new List<Naf>();
            }

            var naf = new Naf
            {
                NafId = tAccount.Naf.NafId,
                NafCode = tAccount.Naf.NafCode,
                NafLabel = tAccount.Naf.NafLabel
            };

            return new List<Naf> { naf };
        }

        public static Statistics MapToStatistics(Dictionary<int, int> countByAccountStatus, Dictionary<string, int> countByContactStatus)
        {
            countByAccountStatus = countByAccountStatus == null || countByAccountStatus.Count == 0 ? new Dictionary<int, int>() : countByAccountStatus;
            countByContactStatus = countByContactStatus == null || countByContactStatus.Count == 0 ? new Dictionary<string, int>() : countByContactStatus;

            return InitStatistic(countByAccountStatus, countByContactStatus);
        }

        public static Statistics InitStatistic(Dictionary<int, int> countByAccountStatus, Dictionary<string, int> countByContactStatus)
        {
            return new Statistics
            {
                AccountToDeploy = countByAccountStatus?.TryGetValue(1, out var toDeploy) == true ? toDeploy : 0,
                AccountInProgress = countByAccountStatus?.TryGetValue(2, out var inProgress) == true ? inProgress : 0,
                AccountConnected = countByAccountStatus?.TryGetValue(3, out var connected) == true ? connected : 0,
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

            existingAccount.HubId = accountDetail.Hub?.HubId;
            existingAccount.Vat = accountDetail.Vat?.System;
            existingAccount.Vattype = accountDetail.Vat?.Type;
        }
    }
}
