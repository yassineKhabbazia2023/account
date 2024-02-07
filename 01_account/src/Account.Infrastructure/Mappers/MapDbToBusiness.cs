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
                IsFavorite = roleConnectedContact != null ? roleConnectedContact?.IsFavorite : false,
                Address = source.TAddress?.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    City = address.City,
                    AddressType = address.AddressType
                }),
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
                    })
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
                EmployeeCount = source.StaffSize,
                CommercialName = source.CommercialName,
                Accounting = new Accounting()
                {
                    FiscalExerciseStartDate = source.FiscalExerciseStartDate,
                    FiscalExerciseDuration = source.FiscalExerciseDuration,
                    AccountingType = source.AccountType,
                    FiscalSystem = source.FiscalSystem,
                    TaxationSystem = source.TaxationSystem,
                },
                Legal = new Legal()
                {
                    LegalName = source.LegalName,
                    Siren = source.ISIN,
                    Siret = source.Siret,
                    LegalForm = source.LegalForm,
                    LegalFormCode = source.LegalFormCode,
                    StaffSizeRange = source.StaffSizeRange,
                    Naf = new List<Naf>()
                    {
                        new Naf()
                        {
                            NafId = source.NafId,
                            NafCode = source.SectorCode,
                            NafLabel = source.ActivityDescription
                        }
                    }
                },
                Vat = new Vat()
                {
                    System = source.VAT,
                    Intra = source.VATIntra,
                    Type = source.VATType,
                },
                Address = source.TAddress.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    Country = address.Country,
                    City = address.City,
                    State = address.State,
                    Street = address.Street,
                    ZipCode = address.ZipCode,
                    AddressType = address.AddressType
                }).ToList(),
                Phone = source.TPhone.Select(phone => new Phone()
                {
                    PhoneId = phone.PhoneId,
                    PhoneNumber = phone.PhoneNumber,
                    Type = phone.Type,
                }).ToList(),
                Hub = new Hub()
                {
                    HubId = source.Hub?.HubId,
                    HubName = source.Hub?.HubName,
                },
                DeploymentPlanning = source.TDeploymentPlanning.Select(deployment => new Deployment()
                {
                    DeploymentId = deployment.DeploymentId,
                    DeploymentDate = deployment.DeploymentDate,
                    Status = deployment.Status,
                }).ToList()
            };
        }
    }
}
