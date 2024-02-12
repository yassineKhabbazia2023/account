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
        public static AccountModel MapTAccountToAccountModel(this TAccount source, int contactId)
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
                accountModel.Address = source.MapAddress();
                accountModel.Owner = roleSignatory?.Contact.MapOwner();
                accountModel.Deployment = source.MapDeploymentPlanning();
            }

            return accountModel;
        }

        public static AccountDetail? MapTAccountToAccountDetail(this TAccount source)
        {
            if (source == null)
            {
                return null;
            }

            var accountDetail = new AccountDetail();
            accountDetail.AccountId = source.AccountId;
            accountDetail.AccountNumber = source.AccountNumber;
            accountDetail.IconName = "icon";
            accountDetail.IsActive = source.IsActive;
            accountDetail.Email = source.Email;
            accountDetail.EmployeeCount = source.StaffSize;
            accountDetail.CommercialName = source.CommercialName;
            accountDetail.Accounting = source.MapAccounting();
            accountDetail.Legal = source.MapLegal();
            accountDetail.Vat = source.MapVat();
            accountDetail.Address = source.MapAddress();
            accountDetail.Phone = source.MapPhone();
            accountDetail.Hub = source.MapHub();
            accountDetail.DeploymentPlanning = source.MapDeploymentPlanning();

            return accountDetail;
        }

        private static Owner? MapOwner(this TContact tContact)
        {
            return tContact == null ? null : new Owner()
            {
                ContactEmail = tContact.ContactEmail,
                FirstName = tContact.FirstName,
                LastName = tContact.LastName
            };
        }

        private static ICollection<Deployment>? MapDeploymentPlanning(this TAccount tAccount)
        {
            return tAccount.TDeploymentPlanning == null ? Array.Empty<Deployment>() : tAccount.TDeploymentPlanning.Select(deployment => new Deployment()
            {
                DeploymentId = deployment.DeploymentId,
                DeploymentDate = deployment.DeploymentDate,
                Status = deployment.Status,
            }).ToList();
        }

        private static ICollection<Phone>? MapPhone(this TAccount tAccount)
        {
            return tAccount.TPhone == null ? Array.Empty<Phone>() : tAccount.TPhone.Select(phone => new Phone()
            {
                PhoneId = phone.PhoneId,
                PhoneNumber = phone.PhoneNumber,
                Type = phone.Type,
            }).ToList();
        }

        private static ICollection<Address>? MapAddress(this TAccount tAccount)
        {
            return tAccount.TAddress == null ? Array.Empty<Address>() : tAccount.TAddress.Select(address => new Address()
            {
                AddressId = address.AddressId,
                Country = address.Country,
                City = address.City,
                State = address.State,
                Street = address.Street,
                ZipCode = address.ZipCode,
                AddressType = address.AddressType
            }).ToList();
        }

        private static Hub MapHub(this TAccount tAccount)
        {
            return new Hub()
            {
                HubId = tAccount.Hub?.HubId,
                HubName = tAccount.Hub?.HubName,
            };
        }

        private static Vat MapVat(this TAccount tAccount)
        {
            return new Vat()
            {
                System = tAccount.VAT,
                Intra = tAccount.VATIntra,
                Type = tAccount.VATType,
            };
        }

        private static Accounting MapAccounting(this TAccount tAccount)
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

        private static Legal MapLegal(this TAccount tAccount)
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
