// <copyright file="MapToAccountEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Globalization;
using AutoFixture;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Mappers.Events
{
    public class MapToAccountEntityTests
    {
        [Fact]
        public void ToAccountEntity_ToUpdate_MapCorrectly()
        {
            // Arrange
            var source = new AccountEntity
            {
                LegalName = "legalName",
                IsActive = true,
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 2
                    }
                },
                CommercialName = "commercialName",
                AccountType = "accountType",
                Email = "accountEmail",
                NafId = 1,
                SectorCode = "sectorCodeNew",
                Vatintra = "vat",
                DeliveryEmail = "delivery@email.com",
                BillingEmail = "billing@email.com",
                TaxationSystem = "taxationSystem",
                SourceName = "sourceName",
                Isin = "ISIN",
                Siret = "registerId1",
                StaffSize = 10,
                DeliveryFax = "deliveryFax",
                BillingFax = "billingFax",
                Turnover = (decimal?)1,
                FiscalSystem = "regimeFiscal",
                AccountingMethod = "typeTenueComptable",
                LegalForm = "formeJuridique",
                StaffSizeRange = "staffSizeSlice",
                ActivityType = "category",
                LegalFormCode = "codeFormeJuridique",
                CreationDate = default,
                UpdatedDate = default,
                CreatedBy = "createdTest",
                ModifiedBy = "modifiedTest",
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressLine1 = "deliveryLine1",
                        AddressLine2 = "deliveryLine2",
                        AddressLine3 = "deliveryLine3",
                        City = "deliveryCity",
                        ZipCode = "deliveryZipCode",
                        Country = "deliveryCountry",
                        State = "deliveryState",
                        AddressType = AddressType.Delivery.ToString()
                    },
                    new AddressEntity
                    {
                        AddressLine1 = "billingLine1New",
                        AddressLine2 = "billingLine2New2",
                        AddressLine3 = "billingLine3",
                        City = "billingCity",
                        ZipCode = "billingZipCOde",
                        Country = "billingCountry",
                        State = "billingState",
                        AddressType = AddressType.Billing.ToString()
                    }
                },
                PhoneEntity = new List<PhoneEntity>
                {
                    new PhoneEntity
                    {
                        PhoneNumber = "123",
                        Type = PhoneType.Delivery.ToString()
                    }
                },
                AccountGlobalUniqueId = new Guid("b67fba5a-2589-4074-b389-543d1e8fce7b"),
                AccountNumber = "accountNUmber"
            };
            var destination = new AccountEntity
            {
                LegalName = "legalName",
                IsActive = true,
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 2
                    }
                },
                CommercialName = "commercialName2",
                AccountType = "accountType2",
                Email = "accountEmail2",
                NafId = 1,
                SectorCode = "sectorCodeNew2",
                Vatintra = "vat",
                DeliveryEmail = "delivery2@email.com",
                BillingEmail = "billing2@email.com",
                TaxationSystem = "taxationSystem2",
                SourceName = "sourceName2",
                Isin = "ISIN2",
                Siret = "registerId12",
                StaffSize = 10,
                DeliveryFax = "deliveryFax2",
                BillingFax = "billingFax2",
                Turnover = (decimal?)1,
                FiscalSystem = "regimeFiscal2",
                AccountingMethod = "typeTenueComptable2",
                LegalForm = "formeJuridique2",
                StaffSizeRange = "staffSizeSlice2",
                ActivityType = "category2",
                LegalFormCode = "codeFormeJuridique2",
                CreationDate = default,
                UpdatedDate = default,
                CreatedBy = "createdTest2",
                ModifiedBy = "modifiedTest2",
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressLine1 = "deliveryLine12",
                        AddressLine2 = "deliveryLine22",
                        AddressLine3 = "deliveryLine32",
                        City = "deliveryCity2",
                        ZipCode = "deliveryZipCode2",
                        Country = "deliveryCountry2",
                        State = "deliveryState2",
                        AddressType = AddressType.Delivery.ToString()
                    },
                    new AddressEntity
                    {
                        AddressLine1 = "billingLine1New2",
                        AddressLine2 = "billingLine2New22",
                        AddressLine3 = "billingLine32",
                        City = "billingCity2",
                        ZipCode = "billingZipCOde2",
                        Country = "billingCountry2",
                        State = "billingState2",
                        AddressType = AddressType.Billing.ToString()
                    }
                },
                PhoneEntity = new List<PhoneEntity> {
                    new PhoneEntity
                    {
                        PhoneNumber = "456",
                        Type = PhoneType.Delivery.ToString()
                    }
                },
                AccountGlobalUniqueId = new Guid("b67fba5a-2589-4074-b389-543d1e8fce7b"),
                AccountNumber = "accountNUmber"
            };

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
            var fixture = new Fixture();
            var data = fixture.Build<RegistryAccountStateEventData>()
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
            Assert.Equal(data.DeploymentStatus, result.DeploymentEntity.First().Status.ToString());
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
            var source = new AccountEntity
            {
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 2,
                        DeploymentDate = DateTime.UtcNow
                    }
                },
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressType = AddressType.Delivery.ToString(),
                        AddressLine1 = "sourceDeliveryLine1"
                    },
                    new AddressEntity
                    {
                        AddressType = AddressType.Billing.ToString(),
                        AddressLine1 = "sourceBillingLine1"
                    }
                },
                PhoneEntity = new List<PhoneEntity>
                {
                    new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString(),
                        PhoneNumber = "sourceDeliveryPhone"
                    },
                    new PhoneEntity
                    {
                        Type = PhoneType.Billing.ToString(),
                        PhoneNumber = "sourceBillingPhone"
                    }
                }
            };
            var destination = new AccountEntity
            {
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 1,
                        DeploymentDate = DateTime.UtcNow.AddDays(-1)
                    }
                },
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressType = AddressType.Delivery.ToString(),
                        AddressLine1 = "destinationDeliveryLine1"
                    },
                    new AddressEntity
                    {
                        AddressType = AddressType.Billing.ToString(),
                        AddressLine1 = "destinationBillingLine1"
                    }
                },
                PhoneEntity = new List<PhoneEntity>
                {
                    new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString(),
                        PhoneNumber = "destinationDeliveryPhone"
                    },
                    new PhoneEntity
                    {
                        Type = PhoneType.Billing.ToString(),
                        PhoneNumber = "destinationBillingPhone"
                    }
                }
            };

            // Act
            destination.ToAccountEntity(source, true);

            // Assert
            Assert.Equal(1, destination.DeploymentEntity.First().Status);
            Assert.NotEqual(source.DeploymentEntity.First().DeploymentDate, destination.DeploymentEntity.First().DeploymentDate);
        }

        [Fact]
        public void ToAccountEntity_CreateOperation_UpdatesDeploymentEntity()
        {
            // Arrange
            var source = new AccountEntity
            {
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 2,
                        DeploymentDate = DateTime.UtcNow
                    }
                },
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressType = AddressType.Delivery.ToString(),
                        AddressLine1 = "sourceDeliveryLine1"
                    },
                    new AddressEntity
                    {
                        AddressType = AddressType.Billing.ToString(),
                        AddressLine1 = "sourceBillingLine1"
                    }
                },
                PhoneEntity = new List<PhoneEntity>
                {
                    new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString(),
                        PhoneNumber = "sourceDeliveryPhone"
                    },
                    new PhoneEntity
                    {
                        Type = PhoneType.Billing.ToString(),
                        PhoneNumber = "sourceBillingPhone"
                    }
                }
            };
            var destination = new AccountEntity
            {
                DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        Status = 1,
                        DeploymentDate = DateTime.UtcNow.AddDays(-1)
                    }
                },
                AddressEntity = new List<AddressEntity>
                {
                    new AddressEntity
                    {
                        AddressType = AddressType.Delivery.ToString(),
                        AddressLine1 = "destinationDeliveryLine1"
                    },
                    new AddressEntity
                    {
                        AddressType = AddressType.Billing.ToString(),
                        AddressLine1 = "destinationBillingLine1"
                    }
                },
                PhoneEntity = new List<PhoneEntity>
                {
                    new PhoneEntity
                    {
                        Type = PhoneType.Delivery.ToString(),
                        PhoneNumber = "destinationDeliveryPhone"
                    },
                    new PhoneEntity
                    {
                        Type = PhoneType.Billing.ToString(),
                        PhoneNumber = "destinationBillingPhone"
                    }
                }
            };

            // Act
            destination.ToAccountEntity(source, false);

            // Assert
            Assert.Equal(2, destination.DeploymentEntity.First().Status);
            Assert.Equal(source.DeploymentEntity.First().DeploymentDate, destination.DeploymentEntity.First().DeploymentDate);
        }
    }
}
