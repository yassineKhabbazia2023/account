using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Kpmg.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapDbToBusiness
    {
        public static AccountModel TAccountToAccountModel(this TAccount source, int contactId)
        {
            var roleSignatory = source.TRoles.FirstOrDefault(role => role.IsSignatory == true);
            var roleConnectedContact = source.TRoles?.FirstOrDefault(role => role.ContactId == contactId);
            return new AccountModel()
            {
                AccountId = source.AccountGlobalUniqueId,
                AccountNumber = source.SourceAccountNumber,
                LegalName = source.LegalName,
                IsFavorite = roleConnectedContact?.IsFavorite,
                Address = source.TAddress?.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    City = address.City,
                    AddressType = address.AddressType
                }).ToList(),
                Owner = new Owner()
                {
                    ContactEmail = roleSignatory?.Contact.ContactEmail,
                    FirstName = roleSignatory?.Contact.FirstName,
                    LastName = roleSignatory?.Contact.LastName
                },
                Deployment = source.TDeploymentPlanning?.Select(deploymentPlanning =>
                    new Deployment()
                    {
                        DeploymentId = deploymentPlanning.DeploymentId,
                        DeploymentDate = deploymentPlanning.DeploymentDate,
                        Status = deploymentPlanning.Status
                    }).ToList()
            };
        }

        public static AccountDetail TAccountToAccountDetail(this TAccount source)
        {
            return new AccountDetail()
            {
                AccountId = source.AccountId,
                AccountNumber = source.SourceAccountNumber,
                IconName = "icon",
                IsActive = source.IsActive,
                Email = source?.Email,
                EmployeeCount = source?.StaffSize,
                CommercialName = source?.CommercialName,
                Accounting = source?.InitAccounting(),
                Legal = source?.InitLegal(),
                Vat = source?.InitVat(),
                Address = source?.TAddress.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    Country = address.Country,
                    City = address.City,
                    State = address.State,
                    Street = address.Street,
                    ZipCode = address.ZipCode,
                    AddressType = address.AddressType
                }).ToList(),
                Phone = source?.TPhone.Select(phone => new Phone()
                {
                    PhoneId = phone.PhoneId,
                    PhoneNumber = phone.PhoneNumber,
                    Type = phone.Type,
                }).ToList(),
                Hub = source?.InitHub(),
                DeploymentPlanning = source?.TDeploymentPlanning.Select(deployment => new Deployment()
                {
                    DeploymentId = deployment.DeploymentId,
                    DeploymentDate = deployment.DeploymentDate,
                    Status = deployment.Status,
                }).ToList()
            };
        }

        private static Hub InitHub(this TAccount tAccount)
        {
            return new Hub()
            {
                HubId = tAccount.Hub?.HubId,
                HubName = tAccount.Hub?.HubName,
            };
        }

        private static Vat InitVat(this TAccount tAccount)
        {
            return new Vat()
            {
                System = tAccount.VAT,
                Intra = tAccount.VATIntra,
                Type = tAccount.VATType,
            };
        }

        private static Accounting InitAccounting(this TAccount tAccount)
        {
            return new Accounting()
            {
                FiscalExerciseStartDate = tAccount.FiscalExerciseStartDate,
                FiscalExerciseDuration = tAccount.FiscalExerciseDuration,
                AccountingType = tAccount.AccountType,
                FiscalSystem = tAccount.FiscalSystem,
                TaxationSystem = tAccount.TaxationSystem,
            };
        }

        private static Legal InitLegal(this TAccount tAccount)
        {
            return new Legal()
            {
                LegalName = tAccount.LegalName,
                Siren = tAccount.ISIN,
                Siret = tAccount.Siret,
                LegalForm = tAccount.LegalForm,
                LegalFormCode = tAccount.LegalFormCode,
                StaffSizeRange = tAccount.StaffSizeRange,
                Naf = new List<Naf>()
                {
                    new Naf()
                    {
                        NafId = tAccount.NafId,
                        NafCode = tAccount.SectorCode,
                        NafLabel = tAccount.ActivityDescription
                    }
                }
            };
        }
    }
}
