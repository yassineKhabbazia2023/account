using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Pulse.Account.Core.Models;
using Microsoft.Identity.Client;
using Pulse.Account.Infrastructure.Entities;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapDbToBusiness
    {
        public static AccountModel TAccountToAccountModel(this TAccount source, int contactId)
        {
            var accountModel = new AccountModel();
            if (source != null)
            {
                var roleSignatory = source.TRoles.FirstOrDefault(role => role.IsSignatory == true);
                var roleConnectedContact = source.TRoles?.FirstOrDefault(role => role.ContactId == contactId);

                accountModel.AccountId = source.AccountId;
                accountModel.AccountNumber = source.AccountNumber;
                accountModel.LegalName = source.LegalName;
                accountModel.IsFavorite = roleConnectedContact?.IsFavorite;
                accountModel.Address = source.TAddress?.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    City = address.City,
                    AddressType = address.AddressType
                }).ToList();
                accountModel.Owner = new Owner()
                {
                    ContactEmail = roleSignatory?.Contact.ContactEmail,
                    FirstName = roleSignatory?.Contact.FirstName,
                    LastName = roleSignatory?.Contact.LastName
                };
                accountModel.Deployment = source.TDeploymentPlanning?.Select(deploymentPlanning => new Deployment()
                {
                    DeploymentId = deploymentPlanning.DeploymentId,
                    DeploymentDate = deploymentPlanning.DeploymentDate,
                    Status = deploymentPlanning.Status
                }).ToList();
            }

            return accountModel;
        }

        public static AccountDetail TAccountToAccountDetail(this TAccount source)
        {
            var accountDetail = new AccountDetail();
            if(source != null)
            {
                accountDetail.AccountId = source.AccountId;
                accountDetail.AccountNumber = source.AccountNumber;
                accountDetail.IconName = "icon";
                accountDetail.IsActive = source.IsActive;
                accountDetail.Email = source.Email;
                accountDetail.EmployeeCount = source.StaffSize;
                accountDetail.CommercialName = source.CommercialName;
                accountDetail.Accounting = source.InitAccounting();
                accountDetail.Legal = source.InitLegal();
                accountDetail.Vat = source.InitVat();
                accountDetail.Address = source.TAddress.Select(address => new Address()
                {
                    AddressId = address.AddressId,
                    Country = address.Country,
                    City = address.City,
                    State = address.State,
                    Street = address.Street,
                    ZipCode = address.ZipCode,
                    AddressType = address.AddressType
                }).ToList();
                accountDetail.Phone = source.TPhone.Select(phone => new Phone()
                {
                    PhoneId = phone.PhoneId,
                    PhoneNumber = phone.PhoneNumber,
                    Type = phone.Type,
                }).ToList();
                accountDetail.Hub = source.InitHub();
                accountDetail.DeploymentPlanning = source.TDeploymentPlanning.Select(deployment => new Deployment()
                {
                    DeploymentId = deployment.DeploymentId,
                    DeploymentDate = deployment.DeploymentDate,
                    Status = deployment.Status,
                }).ToList();
            }

            return accountDetail;
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
