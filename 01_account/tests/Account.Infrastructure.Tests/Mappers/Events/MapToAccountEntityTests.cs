// <copyright file="MapToAccountEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Globalization;
using AutoFixture;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Mappers.Events;

public class MapToAccountEntityTests
{
    private readonly Fixture _fixture;

    public MapToAccountEntityTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void ToAccountEntity_ToUpdate_MapCorrectly()
    {
        // Arrange
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery");
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing");
        var deliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery");
        var billingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing");
        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();
        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();

        // Act
        destination!.ToAccountEntity(source);

        // Assert
        Assert.Equal(destination.LegalName, source.LegalName);
        Assert.Equal(destination.IsActive, source.IsActive);
        Assert.Equal(destination.CommercialName, source.CommercialName);
        Assert.Equal(destination.AccountType, source.AccountType);
        Assert.Equal(destination.Email, source.Email);
        Assert.Equal(destination.NafId, source.NafId);
        Assert.Equal(destination.SectorCode, source.SectorCode);
        Assert.Equal(destination.Vatintra, source.Vatintra);
        Assert.Equal(destination.DeliveryEmail, source.DeliveryEmail);
        Assert.Equal(destination.BillingEmail, source.BillingEmail);
        Assert.Equal(destination.TaxationSystem, source.TaxationSystem);
        Assert.Equal(destination.SourceName, source.SourceName);
        Assert.Equal(destination.Isin, source.Isin);
        Assert.Equal(destination.Siret, source.Siret);
        Assert.Equal(destination.StaffSize, source.StaffSize);
        Assert.Equal(destination.DeliveryFax, source.DeliveryFax);
        Assert.Equal(destination.BillingFax, source.BillingFax);
        Assert.Equal(destination.Turnover, source.Turnover);
        Assert.Equal(destination.FiscalSystem, source.FiscalSystem);
        Assert.Equal(destination.AccountingMethod, source.AccountingMethod);
        Assert.Equal(destination.LegalForm, source.LegalForm);
        Assert.Equal(destination.StaffSizeRange, source.StaffSizeRange);
        Assert.Equal(destination.ActivityType, source.ActivityType);
        Assert.Equal(destination.LegalFormCode, source.LegalFormCode);
        Assert.Equal(destination.CreationDate, source.CreationDate);
        Assert.Equal(destination.UpdatedDate, source.UpdatedDate);
        Assert.Equal(destination.CreatedBy, source.CreatedBy);
        Assert.Equal(destination.ModifiedBy, source.ModifiedBy);
    }

    [Fact]
    public void ToAccountEntity_ToCreated_MapCorrectly()
    {
        // Arrange
        var data = _fixture.Build<RegistryAccountStateEventData>()
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();

        // Act
        var result = data.ToAccountEntity();

        // Assert
        Assert.Equal(data.AccountGlobalUniqueIdentifier, result.AccountGlobalUniqueId);
        Assert.Equal(data.AccountNumber, result.AccountNumber);
        Assert.Equal(data.AccountInsertedDate, result.CreationDate);
        Assert.Equal(data.AccountUpdatedDate, result.UpdatedDate);
        Assert.Equal(data.AccountLegalName, result.LegalName);
        Assert.Equal(data.CreatedBy, result.CreatedBy);
        Assert.Equal(data.ModifiedBy, result.ModifiedBy);
        Assert.Equal(data.AccountCommercialName, result.CommercialName);
        Assert.Equal(data.AccountType, result.AccountType);
        Assert.Equal(data.AccountEmail, result.Email);
        Assert.Equal(data.AccountSectorCode, result.SectorCode);
        Assert.Equal(data.AccountTaxeValeurAjoutee, result.Vatintra);
        Assert.Equal(data.AccountDeliveryEmail, result.DeliveryEmail);
        Assert.Equal(data.AccountBillingEmail, result.BillingEmail);
        Assert.Equal(data.AccountTaxationSystem, result.TaxationSystem);
        Assert.Equal(data.AccountSourceName, result.SourceName);
        Assert.Equal(data.AccountISIN, result.Isin);
        Assert.Equal(data.AccountStaffSize, result.StaffSize.ToString());
        Assert.Equal(data.AccountStaffSizeSlice, result.StaffSizeRange);
        Assert.Equal(data.AccountDeliveryFax, result.DeliveryFax);
        Assert.Equal(data.AccountBillingFax, result.BillingFax);
        Assert.Equal(decimal.Parse(data.Turnover!, CultureInfo.InvariantCulture), result.Turnover);
        Assert.Equal(data.AccountRegimeFiscal, result.FiscalSystem);
        Assert.Equal(data.AccountTypeTenueComptable, result.AccountingMethod);
        Assert.Equal(data.AccountFormeJuridique, result.LegalForm);
        Assert.Equal(data.AccountCodeFormeJuridique, result.LegalFormCode);
        Assert.Equal(data.AccountRegisterIdentification1, result.Siret);
        Assert.Equal(data.AccountNafIdentifier, result.NafId.ToString());
        Assert.Equal(data.AccountFlagESCActif, result.IsActive);
        Assert.Equal(data.AccountEscCategory, result.ActivityType);
        Assert.Equal(data.DeploymentStatus, result.DeploymentEntity.Status.ToString());
        Assert.Equal(data.BillingAddressLine1, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).AddressLine1);
        Assert.Equal(data.BillingAddressLine2, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).AddressLine2);
        Assert.Equal(data.BillingAddressLine3, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).AddressLine3);
        Assert.Equal(data.BillingZipCode, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).ZipCode);
        Assert.Equal(data.BillingCity, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).City);
        Assert.Equal(data.BillingCountry, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).Country);
        Assert.Equal(data.BillingState, result.AddressEntity.First(x => x.AddressType == AddressType.Billing.ToString()).State);
        Assert.Equal(data.DeliveryAddressLine1, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).AddressLine1);
        Assert.Equal(data.DeliveryAddressLine2, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).AddressLine2);
        Assert.Equal(data.DeliveryAddressLine3, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).AddressLine3);
        Assert.Equal(data.DeliveryZipCode, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).ZipCode);
        Assert.Equal(data.DeliveryCity, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).City);
        Assert.Equal(data.DeliveryCountry, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).Country);
        Assert.Equal(data.DeliveryState, result.AddressEntity.First(x => x.AddressType == AddressType.Delivery.ToString()).State);
    }

    [Fact]
    public void ToAccountEntity_UpdateOperation_DoesNotUpdateDeploymentEntity()
    {
        // Arrange
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery");
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing");
        var deliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery");
        var billingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing");

        var source = _fixture.Build<AccountEntity>()
            .With(a => a.DeploymentEntity, new DeploymentEntity
            {
                Status = 2,
                DeploymentDate = DateTime.UtcNow
            })
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(a => a.DeploymentEntity, new DeploymentEntity
            {
                Status = 1,
                DeploymentDate = DateTime.UtcNow.AddDays(-1)
            })
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();

        // Act
        destination.ToAccountEntity(source, false);

        // Assert
        Assert.Equal(1, destination.DeploymentEntity.Status);
        Assert.NotEqual(source.DeploymentEntity.DeploymentDate, destination.DeploymentEntity.DeploymentDate);
    }

    [Fact]
    public void ToAccountEntity_CreateOperation_UpdatesDeploymentEntity()
    {
        // Arrange
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery");
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing");
        var deliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery");
        var billingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing");

        var source = _fixture.Build<AccountEntity>()
            .With(a => a.DeploymentEntity, new DeploymentEntity
            {
                Status = 2,
                DeploymentDate = DateTime.UtcNow
            })
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(a => a.DeploymentEntity, new DeploymentEntity
            {
                Status = 1,
                DeploymentDate = DateTime.UtcNow.AddDays(-1)
            })
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress.Create(), billingAddress.Create() })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { deliveryPhone.Create(), billingPhone.Create() })
            .Create();

        // Act
        destination.ToAccountEntity(source, true);

        // Assert
        Assert.Equal(2, destination.DeploymentEntity.Status);
        Assert.Equal(source.DeploymentEntity.DeploymentDate, destination.DeploymentEntity.DeploymentDate);
    }
}
