// <copyright file="MapToAccountEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Globalization;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper;

public static class MapToAccountEntity
{
    public static void ToAccountEntity(this AccountEntity destination, AccountEntity source, bool isCreateOperation = true)
    {
        destination.AccountGlobalUniqueId = source.AccountGlobalUniqueId;
        destination.AccountNumber = source.AccountNumber;
        destination.CreationDate = source.CreationDate;
        destination.UpdatedDate = source.UpdatedDate;
        destination.LegalName = source.LegalName;
        destination.CreatedBy = source.CreatedBy;
        destination.ModifiedBy = source.ModifiedBy;
        destination.CommercialName = source.CommercialName;
        destination.AccountType = source.AccountType;
        destination.Email = source.Email;
        destination.SectorCode = source.SectorCode;
        destination.Vatintra = source.Vatintra;
        destination.DeliveryEmail = source.DeliveryEmail;
        destination.BillingEmail = source.BillingEmail;
        destination.TaxationSystem = source.TaxationSystem;
        destination.SourceName = source.SourceName;
        destination.Isin = source.Isin;
        destination.StaffSize = source.StaffSize;
        destination.StaffSizeRange = source.StaffSizeRange;
        destination.DeliveryFax = source.DeliveryFax;
        destination.BillingFax = source.BillingFax;
        destination.Turnover = source.Turnover;
        destination.FiscalSystem = source.FiscalSystem;
        destination.AccountingMethod = source.AccountingMethod;
        destination.LegalForm = source.LegalForm;
        destination.LegalFormCode = source.LegalFormCode;
        destination.Siret = source.Siret;
        destination.NafId = source.NafId;
        destination.IsActive = source.IsActive;
        destination.ActivityType = source.ActivityType;

        if (isCreateOperation)
        {
            destination.ToDeploymentEntity(source);
        }

        destination.ToAddressEntity(source);
        destination.ToPhoneEntity(source);
    }

    private static void ToDeploymentEntity(this AccountEntity destination, AccountEntity source)
    {
        var deployment = destination.DeploymentEntity;
        var deploymentNew = source.DeploymentEntity;

        deployment!.DeploymentDate = deploymentNew!.DeploymentDate;
        deployment!.Status = deploymentNew!.Status;
    }

    private static void ToPhoneEntity(this AccountEntity destination, AccountEntity source)
    {
        var phoneDelivery = destination.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Delivery.ToString());
        var phoneBilling = destination.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Billing.ToString());

        var phoneDeliveryNew = source.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Delivery.ToString());
        var phoneBillingNew = source.PhoneEntity.FirstOrDefault(x => x.Type == PhoneType.Billing.ToString());

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

    private static void ToAddressEntity(this AccountEntity destination, AccountEntity source)
    {
        var addressDelivery = destination.AddressEntity.FirstOrDefault(x => AddressType.Delivery.ToString().Equals(x.AddressType, StringComparison.InvariantCultureIgnoreCase));
        var addressDeliveryNew = source.AddressEntity.FirstOrDefault(x => AddressType.Delivery.ToString().Equals(x.AddressType, StringComparison.InvariantCultureIgnoreCase));

        var addressBilling = destination.AddressEntity.FirstOrDefault(x => AddressType.Billing.ToString().Equals(x.AddressType, StringComparison.InvariantCultureIgnoreCase));
        var addressBillingNew = source.AddressEntity.FirstOrDefault(x => AddressType.Billing.ToString().Equals(x.AddressType, StringComparison.InvariantCultureIgnoreCase));

        addressDelivery!.ToAddressEntity(addressDeliveryNew!);
        addressBilling!.ToAddressEntity(addressBillingNew!);
    }

    private static void ToAddressEntity(this AddressEntity destination, AddressEntity source)
    {
        destination.City = source.City;
        destination.Country = source.Country;
        destination.AddressLine1 = source.AddressLine1;
        destination.AddressLine2 = source.AddressLine2;
        destination.AddressLine3 = source.AddressLine3;
        destination.City = source.City;
        destination.ZipCode = source.ZipCode;
        destination.Country = source.Country;
        destination.AddressType = source.AddressType;
        destination.State = source.State;
    }

    public static AccountEntity ToAccountEntity(this RegistryAccountStateEventData eventData, bool isCreateOperation = true)
    {
        if (eventData == null)
        {
            return null!;
        }

        var account = new AccountEntity
        {
            AccountGlobalUniqueId = eventData.AccountGlobalUniqueIdentifier,
            AccountNumber = eventData.AccountNumber,
            CreationDate = eventData.AccountInsertedDate.HasValue ? eventData.AccountInsertedDate.Value : DateTime.UtcNow,
            UpdatedDate = eventData.AccountUpdatedDate,
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
            StaffSize = int.TryParse(eventData.AccountStaffSize!, out int staffSize) ? staffSize : null,
            StaffSizeRange = eventData.AccountStaffSizeSlice,
            DeliveryFax = eventData.AccountDeliveryFax,
            BillingFax = eventData.AccountBillingFax,
            Turnover = decimal.TryParse(eventData.Turnover, CultureInfo.InvariantCulture, out decimal turnover) ? turnover : null,
            FiscalSystem = eventData.AccountRegimeFiscal,
            AccountingMethod = eventData.AccountTypeTenueComptable,
            LegalForm = eventData.AccountFormeJuridique,
            LegalFormCode = eventData.AccountCodeFormeJuridique,
            Siret = eventData.AccountRegisterIdentification1,
            NafId = int.TryParse(eventData.AccountNafIdentifier, out int nafId) ? nafId : null,
            IsActive = true,
            ActivityType = eventData.AccountEscCategory
        };

        if (isCreateOperation)
        {
            account.DeploymentEntity = eventData.ToDeploymentEntities();
        }

        account.AddressEntity = eventData.ToAddressEntities();
        account.PhoneEntity = eventData.ToPhoneEntities();

        return account;
    }

    private static List<PhoneEntity> ToPhoneEntities(this RegistryAccountStateEventData eventData)
    {
        var result = new List<PhoneEntity>();
        if (eventData.BillingPhone != null)
        {
            result.Add(new PhoneEntity
            {
                Type = PhoneType.Billing.ToString(),
                PhoneNumber = eventData.BillingPhone
            });
        }

        if (eventData.DeliveryPhone != null)
        {
            result.Add(new PhoneEntity
            {
                Type = PhoneType.Delivery.ToString(),
                PhoneNumber = eventData.DeliveryPhone
            });
        }

        return result;
    }

    private static List<AddressEntity> ToAddressEntities(this RegistryAccountStateEventData eventData)
    {
        var result = new List<AddressEntity>();

        if (eventData.DeliveryCity != null)
        {
            result.Add(new AddressEntity
            {
                AddressLine1 = eventData.DeliveryAddressLine1,
                AddressLine2 = eventData.DeliveryAddressLine2,
                AddressLine3 = eventData.DeliveryAddressLine3,
                City = eventData.DeliveryCity,
                ZipCode = eventData.DeliveryZipCode,
                Country = eventData.DeliveryCountry,
                AddressType = AddressType.Delivery.ToString(),
                State = eventData.DeliveryState
            });
        }

        if (eventData.BillingCity != null)
        {
            result.Add(new AddressEntity
            {
                AddressLine1 = eventData.BillingAddressLine1,
                AddressLine2 = eventData.BillingAddressLine2,
                AddressLine3 = eventData.BillingAddressLine3,
                City = eventData.BillingCity,
                ZipCode = eventData.BillingZipCode,
                Country = eventData.BillingCountry,
                AddressType = AddressType.Billing.ToString(),
                State = eventData.BillingState
            });
        }

        return result;
    }

    private static DeploymentEntity ToDeploymentEntities(this RegistryAccountStateEventData eventData)
    {
        return new DeploymentEntity
        {
            DeploymentDate = eventData.DeploymentDate,
            Status = int.TryParse(eventData.DeploymentStatus, out int status) ? status : (int)DeploymentStatus.ToDeploy,
        };
    }
}
