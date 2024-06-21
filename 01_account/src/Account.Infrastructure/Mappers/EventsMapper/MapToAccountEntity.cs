// <copyright file="MapToAccountEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper
{
    public static class MapToAccountEntity
    {
        public static void ToAccountEntity(this AccountEntity source, AccountEntity destination)
        {
            source.AccountGlobalUniqueId = destination.AccountGlobalUniqueId;
            source.AccountNumber = destination.AccountNumber;
            source.CreationDate = destination.CreationDate;
            source.UpdatedDate = destination.UpdatedDate;
            source.LegalName = destination.LegalName;
            source.CreatedBy = destination.CreatedBy;
            source.ModifiedBy = destination.ModifiedBy;
            source.CommercialName = destination.CommercialName;
            source.AccountType = destination.AccountType;
            source.Email = destination.Email;
            source.SectorCode = destination.SectorCode;
            source.Vatintra = destination.Vatintra;
            source.DeliveryEmail = destination.DeliveryEmail;
            source.BillingEmail = destination.BillingEmail;
            source.TaxationSystem = destination.TaxationSystem;
            source.SourceName = destination.SourceName;
            source.Isin = destination.Isin;
            source.StaffSize = destination.StaffSize;
            source.StaffSizeRange = destination.StaffSizeRange;
            source.DeliveryFax = destination.DeliveryFax;
            source.BillingFax = destination.BillingFax;
            source.Turnover = destination.Turnover;
            source.FiscalSystem = destination.FiscalSystem;
            source.AccountingMethod = destination.AccountingMethod;
            source.LegalForm = destination.LegalForm;
            source.LegalFormCode = destination.LegalFormCode;
            source.Siret = destination.Siret;
            source.NafId = destination.NafId;
            source.IsActive = destination.IsActive;
            source.ActivityType = destination.ActivityType;
            source.ToDeploymentEntity(destination);
            source.ToAddressEntity(destination);
            source.ToPhoneEntity(destination);
        }

        private static void ToDeploymentEntity(this AccountEntity source, AccountEntity destination)
        {
            var deployment = source.DeploymentEntity.FirstOrDefault();
            var deploymentNew = destination.DeploymentEntity.FirstOrDefault();

            deployment!.DeploymentDate = deploymentNew!.DeploymentDate;
            deployment!.Status = deploymentNew!.Status;
        }

        private static void ToPhoneEntity(this AccountEntity source, AccountEntity destination)
        {
            var phoneDelivery = source.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Delivery.ToString());
            var phoneBilling = source.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Billing.ToString());

            var phoneDeliveryNew = destination.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Delivery.ToString());
            var phoneBillingNew = destination.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Billing.ToString());

            if (phoneDeliveryNew != null)
            {
                if (phoneDelivery == null)
                {
                    phoneDelivery = new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString()
                    };
                }

                phoneDelivery.PhoneNumber = phoneDeliveryNew!.PhoneNumber;
            }

            if (phoneBillingNew != null)
            {
                if (phoneBilling == null)
                {
                    phoneBilling = new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString()
                    };
                }

                phoneBilling!.PhoneNumber = phoneBillingNew!.PhoneNumber;
            }
        }

        private static void ToAddressEntity(this AccountEntity source, AccountEntity destination)
        {
            var addressDelivery = source.AddressEntity.FirstOrDefault(x => x.AddressType == AddressType.delivery.ToString());
            var addressDeliveryNew = destination.AddressEntity.FirstOrDefault(x => x.AddressType == AddressType.delivery.ToString());

            var addressBilling = source.AddressEntity.FirstOrDefault(x => x.AddressType == AddressType.billing.ToString());
            var addressBillingNew = destination.AddressEntity.FirstOrDefault(x => x.AddressType == AddressType.billing.ToString());

            addressDelivery!.ToAddressEntity(addressDeliveryNew!);
            addressBilling!.ToAddressEntity(addressBillingNew!);
        }

        private static void ToAddressEntity(this AddressEntity source, AddressEntity destination)
        {
            source.City = destination.City;
            source.Country = destination.Country;
            source.AddressLine1 = destination.AddressLine1;
            source.AddressLine2 = destination.AddressLine2;
            source.AddressLine3 = destination.AddressLine3;
            source.City = destination.City;
            source.ZipCode = destination.ZipCode;
            source.Country = destination.Country;
            source.AddressType = destination.AddressType;
            source.State = destination.State;
        }

        public static AccountEntity ToAccountEntity(this RegistryAccountStateEventData eventData)
        {
            if (eventData == null)
            {
                return null!;
            }

            var account = new AccountEntity
            {
                AccountGlobalUniqueId = eventData.AccountGlobalUniqueIdentifier,
                AccountNumber = eventData.AccountNumber,
                CreationDate = eventData.AccountInsertedDate!.Value,
                UpdatedDate = eventData.AccountUpdatedDate!.Value,
                LegalName = eventData.AccountLegalName,
                CreatedBy = eventData.CreatedBy,
                ModifiedBy = eventData.ModifiedBy,
                CommercialName = eventData.AccountCommercialName,
                AccountType = eventData.AccountType,
                Email = eventData.AccountEmail,
                SectorCode = eventData.AccountSectorCode,
                Vatintra = eventData.AccountTaxeValeurAjoutee,
                DeliveryEmail = eventData.AccountDeliveryEmail,
                BillingEmail = eventData.AccountBillingEmail,
                TaxationSystem = eventData.AccountTaxationSystem,
                SourceName = eventData.AccountSourceName,
                Isin = eventData.AccountISIN,
                StaffSize = int.TryParse(eventData.AccountStaffSize!, out int staffSize) ? staffSize : 0,
                StaffSizeRange = eventData.AccountStaffSizeSlice,
                DeliveryFax = eventData.AccountDeliveryFax,
                BillingFax = eventData.AccountBillingFax,
                Turnover = decimal.TryParse(eventData.AccountTurnoverSlice, out decimal turnover) ? turnover : 0,
                FiscalSystem = eventData.AccountRegimeFiscal,
                AccountingMethod = eventData.AccountTypeTenueComptable,
                LegalForm = eventData.AccountFormeJuridique,
                LegalFormCode = eventData.AccountCodeFormeJuridique,
                Siret = eventData.AccountRegisterIdentification1,
                NafId = int.TryParse(eventData.AccountNafIdentifier, out int nafId) ? nafId : throw new BadRequestException(Errors.BadRequestNafIdCode, Errors.BadRequestNafIdMessage),
                IsActive = eventData.AccountFlagESCActif,
                ActivityType = eventData.AccountEscCategory
            };

            account.DeploymentEntity = eventData.ToDeploymentEntities();

            account.AddressEntity = eventData.ToAddressEntities();

            account.PhoneEntity = eventData.ToPhoneEntities();

            return account;
        }

        private static List<PhoneEntity> ToPhoneEntities(this RegistryAccountStateEventData eventData)
        {
            return new List<PhoneEntity>
            {
                new PhoneEntity
                {
                    Type = PhoneType.Delivery.ToString(),
                    PhoneNumber = "012345679"
                },
                new PhoneEntity
                {
                    Type = PhoneType.Billing.ToString(),
                    PhoneNumber = "012345679"
                }
            };
        }

        private static List<AddressEntity> ToAddressEntities(this RegistryAccountStateEventData eventData)
        {
            return new List<AddressEntity>
            {
                new AddressEntity
                {
                    AddressLine1 = eventData.DeliveryAddressLine1,
                    AddressLine2 = eventData.DeliveryAddressLine2,
                    AddressLine3 = eventData.DeliveryAddressLine3,
                    City = eventData.DeliveryCity,
                    ZipCode = eventData.DeliveryZipCode,
                    Country = eventData.DeliveryCountry,
                    AddressType = AddressType.delivery.ToString(),
                    State = eventData.DeliveryState
                },
                new AddressEntity
                {
                    AddressLine1 = eventData.BillingAddressLine1,
                    AddressLine2 = eventData.BillingAddressLine2,
                    AddressLine3 = eventData.BillingAddressLine3,
                    City = eventData.BillingCity,
                    ZipCode = eventData.BillingZipCode,
                    Country = eventData.BillingCountry,
                    AddressType = AddressType.billing.ToString(),
                    State = eventData.BillingState
                }
            };
        }

        private static List<DeploymentEntity> ToDeploymentEntities(this RegistryAccountStateEventData eventData)
        {
            return new List<DeploymentEntity>
            {
                new DeploymentEntity
                {
                    DeploymentDate = eventData.DeploymentDate!.Value,
                    Status = int.TryParse(eventData.DeploymentStatus, out int status) ? status : (int)DeploymentStatus.ToDeploy,
                }
            };
        }
    }
}
