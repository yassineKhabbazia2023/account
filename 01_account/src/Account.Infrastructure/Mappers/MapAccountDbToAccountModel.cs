// <copyright file="MapAccountDbToAccountModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Azure;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapAccountDbToAccountModel
    {
        public static Paging<Core.Models.Account> MapToPaginAccounts(
            this ICollection<TAccount> source,
            int contactId,
            int pageNumber,
            int totalRows,
            float totalPageCalcul)
        {
            return
             new Paging<Core.Models.Account>()
             {
                 Items = source.MapToAccounts(contactId),
                 CurrentPage = pageNumber,
                 TotalItems = totalRows,
                 TotalPage = (int)Math.Ceiling(totalPageCalcul)
             };
        }

        public static IEnumerable<Core.Models.Account> MapToAccounts(this ICollection<TAccount> source, int contactId)
        {
            return source?.Select(a => a.MapToAccount(contactId) !) ?? Enumerable.Empty<Core.Models.Account>();
        }

        public static Core.Models.Account? MapToAccount(this TAccount source, int contactId)
        {
            if (source == null)
            {
                return null;
            }

            var signatory = source.TRole?.FirstOrDefault(r => r.IsSignatory == true);
            var currentContact = source.TRole?.FirstOrDefault(r => r.ContactId == contactId);

            return
                new Core.Models.Account
                {
                    AccountId = source.AccountId,
                    AccountNumber = source.AccountNumber,
                    LegalName = source.LegalName,
                    IsFavorite = currentContact?.IsFavorite,
                    Address = source.MapToAddress(),
                    Signatory = signatory?.Contact.MapToContact(),
                    Deployment = source.MapToDeploymentPlanning(),
                };
        }

        public static AccountDetail? MapToAccountDetail(this TAccount source)
        {
            return source == null ? null :
            new AccountDetail
            {
                AccountId = source.AccountId,
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
                DeploymentPlanning = source.MapToDeploymentPlanning()
            };
        }

        private static Contact? MapToContact(this TContact tContact)
        {
            return tContact == null ? null : new Contact
            {
                ContactEmail = tContact.ContactEmail,
                FirstName = tContact.FirstName,
                LastName = tContact.LastName
            };
        }

        private static IEnumerable<Deployment>? MapToDeploymentPlanning(this TAccount tAccount)
        {
            return tAccount.TDeploymentPlanning == null ? Array.Empty<Deployment>() :
                tAccount.TDeploymentPlanning.Select(deployment => new Deployment
                {
                    DeploymentId = deployment.DeploymentId,
                    DeploymentDate = deployment.DeploymentDate,
                    Status = deployment.Status,
                });
        }

        private static IEnumerable<Phone>? MapToPhone(this TAccount tAccount)
        {
            return tAccount.TPhone == null ? Array.Empty<Phone>() :
                tAccount.TPhone.Select(phone => new Phone
                {
                    PhoneId = phone.PhoneId,
                    PhoneNumber = phone.PhoneNumber,
                    Type = phone.Type,
                });
        }

        private static IEnumerable<Address>? MapToAddress(this TAccount tAccount)
        {
            return tAccount.TAddress == null ? Array.Empty<Address>() :
                tAccount.TAddress.Select(address => new Address
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

        private static Hub MapToHub(this TAccount tAccount)
        {
            return new Hub()
            {
                HubId = tAccount.Hub?.HubId,
                HubName = tAccount.Hub?.HubName,
            };
        }

        private static Vat MapToVat(this TAccount tAccount)
        {
            return new Vat()
            {
                System = tAccount.VAT,
                Intra = tAccount.VATIntra,
                Type = tAccount.VATType,
            };
        }

        private static Accounting MapToAccounting(this TAccount tAccount)
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

        private static Legal MapToLegal(this TAccount tAccount)
        {
            return new Legal
            {
                LegalName = tAccount.LegalName,
                Siren = tAccount.ISIN,
                Siret = tAccount.Siret,
                LegalForm = tAccount.LegalForm,
                LegalFormCode = tAccount.LegalFormCode,
                StaffSizeRange = tAccount.StaffSizeRange,
                Naf = tAccount.MapToNaf(),
            };
        }

        private static List<Naf> MapToNaf(this TAccount tAccount)
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

        public static Statistics MapToStatistics(Dictionary<int, int> countByStatus)
        {
            if (countByStatus == null || countByStatus.Count == 0)
            {
                return new Statistics();
            }

            return new Statistics
            {
                AccountToDeploy = countByStatus.TryGetValue(0, out var toDeploy) ? toDeploy : 0,
                AccountInProgress = countByStatus.TryGetValue(1, out var inProgress) ? inProgress : 0,
                AccountConnected = countByStatus.TryGetValue(2, out var connected) ? connected : 0
            };
        }

        public static void MapToUpdatedAccount(this TAccount existingAccount, AccountDetail accountDetail)
        {
            if (accountDetail.Legal != null)
            {
                existingAccount.LegalForm = accountDetail.Legal.LegalForm;
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
            existingAccount.VAT = accountDetail.Vat?.System;
            existingAccount.VATType = accountDetail.Vat?.Type;
        }
    }
}
