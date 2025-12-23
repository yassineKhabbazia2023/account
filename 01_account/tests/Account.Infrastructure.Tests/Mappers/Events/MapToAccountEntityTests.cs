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

        destination!.ToAccountEntity(source);

        Assert.Equal(source.LegalName, destination.LegalName);
        Assert.Equal(source.IsActive, destination.IsActive);
        Assert.Equal(source.CommercialName, destination.CommercialName);
        Assert.Equal(source.AccountType, destination.AccountType);
        Assert.Equal(source.Email, destination.Email);
        Assert.Equal(source.NafId, destination.NafId);
        Assert.Equal(source.SectorCode, destination.SectorCode);
        Assert.Equal(source.Vatintra, destination.Vatintra);
        Assert.Equal(source.DeliveryEmail, destination.DeliveryEmail);
        Assert.Equal(source.BillingEmail, destination.BillingEmail);
        Assert.Equal(source.TaxationSystem, destination.TaxationSystem);
        Assert.Equal(source.SourceName, destination.SourceName);
        Assert.Equal(source.Isin, destination.Isin);
        Assert.Equal(source.Siret, destination.Siret);
        Assert.Equal(source.StaffSize, destination.StaffSize);
        Assert.Equal(source.DeliveryFax, destination.DeliveryFax);
        Assert.Equal(source.BillingFax, destination.BillingFax);
        Assert.Equal(source.Turnover, destination.Turnover);
        Assert.Equal(source.FiscalSystem, destination.FiscalSystem);
        Assert.Equal(source.AccountingMethod, destination.AccountingMethod);
        Assert.Equal(source.LegalForm, destination.LegalForm);
        Assert.Equal(source.StaffSizeRange, destination.StaffSizeRange);
        Assert.Equal(source.ActivityType, destination.ActivityType);
        Assert.Equal(source.LegalFormCode, destination.LegalFormCode);
        Assert.Equal(source.CreationDate, destination.CreationDate);
        Assert.Equal(source.UpdatedDate, destination.UpdatedDate);
        Assert.Equal(source.CreatedBy, destination.CreatedBy);
        Assert.Equal(source.ModifiedBy, destination.ModifiedBy);

        var destDeliveryAddress = destination.AddressEntity.First(x => x.AddressType == "Delivery");
        var srcDeliveryAddress = source.AddressEntity.First(x => x.AddressType == "Delivery");
        Assert.Equal(srcDeliveryAddress.AddressLine1, destDeliveryAddress.AddressLine1);
        Assert.Equal(srcDeliveryAddress.City, destDeliveryAddress.City);
        Assert.Equal(srcDeliveryAddress.Country, destDeliveryAddress.Country);

        var destBillingAddress = destination.AddressEntity.First(x => x.AddressType == "Billing");
        var srcBillingAddress = source.AddressEntity.First(x => x.AddressType == "Billing");
        Assert.Equal(srcBillingAddress.AddressLine1, destBillingAddress.AddressLine1);
        Assert.Equal(srcBillingAddress.City, destBillingAddress.City);
        Assert.Equal(srcBillingAddress.Country, destBillingAddress.Country);

        var destDeliveryPhone = destination.PhoneEntity.First(x => x.Type == "Delivery");
        var srcDeliveryPhone = source.PhoneEntity.First(x => x.Type == "Delivery");
        Assert.Equal(srcDeliveryPhone.PhoneNumber, destDeliveryPhone.PhoneNumber);
        Assert.Equal("Delivery", destDeliveryPhone.Type);

        var destBillingPhone = destination.PhoneEntity.First(x => x.Type == "Billing");
        var srcBillingPhone = source.PhoneEntity.First(x => x.Type == "Billing");
        Assert.Equal(srcBillingPhone.PhoneNumber, destBillingPhone.PhoneNumber);
        Assert.Equal("Billing", destBillingPhone.Type);
    }

    [Fact]
    public void ToAccountEntity_ToCreated_MapCorrectly()
    {
        var data = _fixture.Build<RegistryAccountStateEventData>()
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();

        var result = data.ToAccountEntity();

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
        Assert.True(result.IsActive);
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

        destination.ToAccountEntity(source, false);

        Assert.Equal(1, destination.DeploymentEntity.Status);
        Assert.NotEqual(source.DeploymentEntity.DeploymentDate, destination.DeploymentEntity.DeploymentDate);
    }

    [Fact]
    public void ToAccountEntity_CreateOperation_UpdatesDeploymentEntity()
    {
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

        destination.ToAccountEntity(source);

        Assert.Equal(2, destination.DeploymentEntity.Status);
        Assert.Equal(source.DeploymentEntity.DeploymentDate, destination.DeploymentEntity.DeploymentDate);
    }

    [Fact]
    public void ToAccountEntity_ShouldNotOverwriteTurnoverWithNull()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .With(x => x.City, "DeliveryCity")
            .Create();

        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .With(x => x.City, "BillingCity")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.Turnover, (decimal?)null)
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.Turnover, 1000000.50m)
            .Create();

        destination.ToAccountEntity(source);

        Assert.NotNull(destination.Turnover);
        Assert.Equal(1000000.50m, destination.Turnover);
    }

    [Fact]
    public void ToAccountEntity_WhenBillingPhoneDoesNotExist_ShouldCreateWithCorrectType()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .Create();

        var newBillingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing")
            .With(x => x.PhoneNumber, "123-456-7890")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { newBillingPhone })
            .Create();

        var destinationAccountId = _fixture.Create<int>();
        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, destinationAccountId)
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .Create();

        destination.ToAccountEntity(source, false);

        var billingPhone = destination.PhoneEntity.FirstOrDefault(x => x.Type == "Billing");
        Assert.NotNull(billingPhone);
        Assert.Equal("Billing", billingPhone.Type);
        Assert.Equal("123-456-7890", billingPhone.PhoneNumber);
        Assert.Equal(destinationAccountId, billingPhone.AccountId);
    }

    [Fact]
    public void ToAccountEntity_WhenDeliveryPhoneDoesNotExist_ShouldCreateWithCorrectType()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .Create();

        var newDeliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery")
            .With(x => x.PhoneNumber, "987-654-3210")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { newDeliveryPhone })
            .Create();

        var destinationAccountId = _fixture.Create<int>();
        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, destinationAccountId)
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .Create();

        destination.ToAccountEntity(source, false);

        var deliveryPhone = destination.PhoneEntity.FirstOrDefault(x => x.Type == "Delivery");
        Assert.NotNull(deliveryPhone);
        Assert.Equal("Delivery", deliveryPhone.Type);
        Assert.Equal("987-654-3210", deliveryPhone.PhoneNumber);
        Assert.Equal(destinationAccountId, deliveryPhone.AccountId);
    }

    [Fact]
    public void ToAccountEntity_WhenBillingPhoneExists_ShouldUpdatePhoneNumber()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .Create();

        var existingBillingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing")
            .With(x => x.PhoneNumber, "OLD-NUMBER")
            .Create();

        var newBillingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing")
            .With(x => x.PhoneNumber, "NEW-NUMBER")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { newBillingPhone })
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { existingBillingPhone })
            .Create();

        destination.ToAccountEntity(source, false);

        Assert.Single(destination.PhoneEntity, x => x.Type == "Billing");
        var billingPhone = destination.PhoneEntity.First(x => x.Type == "Billing");
        Assert.Equal("NEW-NUMBER", billingPhone.PhoneNumber);
    }

    [Fact]
    public void ToAccountEntity_WhenDeliveryPhoneExists_ShouldUpdatePhoneNumber()
    {
        // Arrange
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .Create();

        var existingDeliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery")
            .With(x => x.PhoneNumber, "OLD-DELIVERY")
            .Create();

        var newDeliveryPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Delivery")
            .With(x => x.PhoneNumber, "NEW-DELIVERY")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { newDeliveryPhone })
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { existingDeliveryPhone })
            .Create();

        destination.ToAccountEntity(source, false);

        Assert.Single(destination.PhoneEntity, x => x.Type == "Delivery");
        var deliveryPhone = destination.PhoneEntity.First(x => x.Type == "Delivery");
        Assert.Equal("NEW-DELIVERY", deliveryPhone.PhoneNumber);
    }

    [Fact]
    public void ToAccountEntity_WhenSourceHasNoPhones_ShouldNotModifyDestinationPhones()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();
        var billingAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Billing")
            .Create();

        var existingPhone = _fixture.Build<PhoneEntity>()
            .With(x => x.Type, "Billing")
            .With(x => x.PhoneNumber, "EXISTING")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress, billingAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity> { existingPhone })
            .Create();

        destination.ToAccountEntity(source, false);

        Assert.Single(destination.PhoneEntity);
        Assert.Equal("EXISTING", destination.PhoneEntity.First().PhoneNumber);
    }

    [Fact]
    public void ToAccountEntity_WhenAddressesAreMissing_ShouldHandleGracefully()
    {
        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity>())
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.DeploymentEntity, _fixture.Create<DeploymentEntity>())
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity>())
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.DeploymentEntity, _fixture.Create<DeploymentEntity>())
            .Create();

        destination.ToAccountEntity(source, false);

        Assert.Equal(source.LegalName, destination.LegalName);
        Assert.Equal(source.AccountNumber, destination.AccountNumber);
    }

    [Fact]
    public void ToAccountEntity_WhenOnlyDeliveryAddressExists_ShouldNotThrow()
    {
        var deliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { deliveryAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.DeploymentEntity, _fixture.Create<DeploymentEntity>())
            .Create();

        var destDeliveryAddress = _fixture.Build<AddressEntity>()
            .With(x => x.AddressType, "Delivery")
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { destDeliveryAddress })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .With(x => x.DeploymentEntity, _fixture.Create<DeploymentEntity>())
            .Create();

        destination.ToAccountEntity(source, false);

        var resultDelivery = destination.AddressEntity.FirstOrDefault(x => x.AddressType == "Delivery");
        Assert.NotNull(resultDelivery);
        Assert.Equal(deliveryAddress.City, resultDelivery.City);
        Assert.Equal(deliveryAddress.AddressLine1, resultDelivery.AddressLine1);
    }

    [Fact]
    public void ToAccountEntity_AddressUpdate_ShouldMapAllAddressFields()
    {
        var sourceDelivery = new AddressEntity
        {
            AddressType = "Delivery",
            AddressLine1 = "123 Source Street",
            AddressLine2 = "Suite 100",
            AddressLine3 = "Building A",
            City = "SourceCity",
            ZipCode = "12345",
            Country = "SourceCountry",
            State = "SourceState"
        };

        var sourceBilling = new AddressEntity
        {
            AddressType = "Billing",
            AddressLine1 = "456 Billing Ave",
            AddressLine2 = "Floor 2",
            AddressLine3 = "Wing B",
            City = "BillingCity",
            ZipCode = "67890",
            Country = "BillingCountry",
            State = "BillingState"
        };

        var destDelivery = new AddressEntity
        {
            AddressType = "Delivery",
            AddressLine1 = "Old Delivery",
            City = "OldCity"
        };

        var destBilling = new AddressEntity
        {
            AddressType = "Billing",
            AddressLine1 = "Old Billing",
            City = "OldBillingCity"
        };

        var source = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { sourceDelivery, sourceBilling })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .Create();

        var destination = _fixture.Build<AccountEntity>()
            .With(x => x.AddressEntity, new List<AddressEntity> { destDelivery, destBilling })
            .With(x => x.PhoneEntity, new List<PhoneEntity>())
            .Create();

        destination.ToAccountEntity(source, false);

        var resultDelivery = destination.AddressEntity.First(x => x.AddressType == "Delivery");
        Assert.Equal("123 Source Street", resultDelivery.AddressLine1);
        Assert.Equal("Suite 100", resultDelivery.AddressLine2);
        Assert.Equal("Building A", resultDelivery.AddressLine3);
        Assert.Equal("SourceCity", resultDelivery.City);
        Assert.Equal("12345", resultDelivery.ZipCode);
        Assert.Equal("SourceCountry", resultDelivery.Country);
        Assert.Equal("SourceState", resultDelivery.State);

        var resultBilling = destination.AddressEntity.First(x => x.AddressType == "Billing");
        Assert.Equal("456 Billing Ave", resultBilling.AddressLine1);
        Assert.Equal("Floor 2", resultBilling.AddressLine2);
        Assert.Equal("Wing B", resultBilling.AddressLine3);
        Assert.Equal("BillingCity", resultBilling.City);
        Assert.Equal("67890", resultBilling.ZipCode);
        Assert.Equal("BillingCountry", resultBilling.Country);
        Assert.Equal("BillingState", resultBilling.State);
    }
}
