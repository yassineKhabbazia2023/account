// <copyright file="MapAccountDbToAccountModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class MapAccountDbToAccountModelTests
{
    [Fact]
    public void MapToPaginAccounts_ShouldMapCorrectly()
    {
        // Arrange

        var contactEntity = new ContactEntity
        {
            Type = "1",
            FirstName = "firstUser",
            LastName = "lastUser",
            Email = "firstLastUser@test.fr",
            CreationDate = DateTime.Now,
            PersonaName = "toto",
        };

        var roleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        IsSignatory = true,
                        Contact = contactEntity,
                    }
                };

        var firstAccountEntity = new AccountEntity
        {
            RoleEntity = roleEntity,
            AccountNumber = "199900046522",
            LegalName = "test scA",
            Hub = new HubEntity { HubId = 1, HubName = "HubName" },
            CreatedBy = "me",
        };

        var secondAccountEntity = new AccountEntity
        {
            RoleEntity = roleEntity,
            AccountNumber = "9876200046522",
            LegalName = "test scA",
            Hub = new HubEntity { HubId = 1, HubName = "HubName" },
            CreatedBy = "me",
        };

        var firstAccountModel = new Core.Models.Account()
        {
            AccountNumber = "199900046522",
            LegalName = "test scA",
            Hub = new Hub() { HubId = 1, HubName = "HubName" }
        };

        var secondAccountModel = new Core.Models.Account()
        {
            AccountNumber = "9876200046522",
            LegalName = "test scA",
            Hub = new Hub { HubId = 1, HubName = "HubName" },
        };

        var source = new List<AccountEntity>
        {
            firstAccountEntity, secondAccountEntity
        };
        int? contactId = 123;
        int pageNumber = 1;
        int totalRows = 10;
        int totalPageCalcul = 2;

        // Mock the MapToAccounts extension method
        var mockMapper = new Mock<IAccountMapper>();
        mockMapper.Setup(m => m.MapToAccounts(It.IsAny<ICollection<AccountEntity>>(), It.IsAny<int?>()))
            .Returns(new List<Core.Models.Account>
            {
              firstAccountModel,
              secondAccountModel
            });

        // Act
        var result = source.MapToPaginAccounts(contactId, pageNumber, totalRows, totalPageCalcul);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Paging<Core.Models.Account>>();
        result.CurrentPage.Should().Be(pageNumber);
        result.TotalItems.Should().Be(totalRows);
        result.TotalPage.Should().Be(totalPageCalcul);
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public void MapToPagingContact_ShouldMapCorrectly()
    {
        // arrange
        Contact firstContact = new Contact()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "mdibeh@hakouna.com",
            FirstName = "Marc",
            LastName = "Dibeh",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata"
        };

        Contact secondContact = new Contact()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "koli@haj.fr",
            FirstName = "Jaji",
            LastName = "dajaja",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata"
        };
        List<Contact> contacts = new List<Contact>()
    {
        firstContact, secondContact
    };

        // act
        var result = contacts.MapToPagingContact(1, 2, 2);

        var pagingContact = new Paging<Contact>()
        {
            Items = contacts,
            CurrentPage = 1,
            TotalItems = 2,
            TotalPage = 2
        };
        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Paging<Contact>>();
        result.Should().BeEquivalentTo(pagingContact);
    }

    [Fact]
    public void MapToContact_ShouldMapCorrectly()
    {
        // arrange
        var label = new LabelEntity
        {
            LabelId = 1,
            Code = "EXPERT_CONSEIL",
            CollaboratorLabel = "Expert conseil",
            CustomerLabel = "Conseiller dédié",
            Business = "ESC",
            Description = "description",
            IsVisible = true,
        };
        var role = new RoleEntity
        {
            AccountId = 1,
            IsCustomerRelation = true,
            ActionLevel = 1,
            ContactFlagPortailFactures = true,
        };
        var contactEntity = new ContactEntity
        {
            ContactId = 1,
            ContactGlobalUniqueId = Guid.NewGuid(),
            Office = "PULSE",
            FirstName = "firstUser",
            LastName = "lastUser",
            Email = "firstLastUser@test.fr",
            CreationDate = DateTime.Now,
            PersonaName = "toto",
            Status = "Active",
            RoleEntity = new List<RoleEntity>
            {
                role
            },
            RoleLabelEntityContact = new List<RoleLabelEntity>
            {
                new()
                {
                    Label = label,
                    AccountId = 1,
                }
            }
        };

        // act
        var result = contactEntity.MapToContact(1);

        // assert
        Assert.NotNull(result);
        Assert.IsType<Contact>(result);
        Assert.Equal(contactEntity.ContactId, result.ContactId);
        Assert.Equal(contactEntity.ContactGlobalUniqueId, result.ContactGlobalUniqueId);
        Assert.Equal(contactEntity.FirstName, result.FirstName);
        Assert.Equal(contactEntity.LastName, result.LastName);
        Assert.Equal(contactEntity.Email, result.Email);
        Assert.Equal(contactEntity.LandPhone, result.LandPhone);
        Assert.Equal(contactEntity.MobilePhone, result.MobilePhone);
        Assert.Equal(contactEntity.Status, result.Status);
        Assert.Equal(contactEntity.Office, result.Office);
        Assert.Equal(contactEntity.CreationDate, result.CreationDate);
        Assert.Equal(contactEntity.PersonaName, result.PersonaName);
        Assert.Equal(role.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(role.ActionLevel, result.ActionLevel);
        Assert.Single(result.Labels!);
        Assert.Equal(label.Code, result.Labels!.First().Code);
        Assert.Equal(label.CollaboratorLabel, result.Labels!.First().CollaboratorLabel);
        Assert.Equal(label.CustomerLabel, result.Labels!.First().CustomerLabel);
        Assert.Equal(label.Business, result.Labels!.First().Business);
        Assert.Equal(label.Description, result.Labels!.First().Description);
        Assert.True(result.ContactFlagPortailFactures!.Value);
    }

    [Fact]
    public void MapToContact_WithNullSource_ShouldReturnNull()
    {
        var result = MapAccountDatabaseToAccountModel.MapToContact(null!, null);

        Assert.Null(result);
    }

    [Fact]
    public void MapToDeployment_ShouldReturnDefaultDeplymentIfDeploymentEntityIsNull()
    {
        // arrange
        var accountEntity = new AccountEntity
        {
            DeploymentEntity = null,
            AccountNumber = "199900046522",
            LegalName = "test scA",
            Hub = new HubEntity { HubId = 1, HubName = "HubName" },
            CreatedBy = "me",
        };

        // act
        var deployment = accountEntity.MapToDeployment();

        deployment.Should().BeEquivalentTo(new Deployment());
    }

    [Fact]
    public void MapToDeployment_ShouldMapCorrectly()
    {
        // arrange
        var deployment = new DeploymentEntity() { Status = 1, DeploymentDate = DateTime.UtcNow, DeploymentId = 1 };
        var accountEntity = new AccountEntity
        {
            DeploymentEntity = deployment,
            AccountNumber = "199900046522",
            LegalName = "test scA",
            Hub = new HubEntity { HubId = 1, HubName = "HubName" },
            CreatedBy = "me",
        };

        // act
        var result = accountEntity.MapToDeployment();

        result.Should().NotBeNull();
        result.Status.Should().Be(deployment.Status);
        result.DeploymentDate.Should().Be(deployment.DeploymentDate);
        result.DeploymentId.Should().Be(deployment.DeploymentId);
    }

    [Fact]
    public void MapToUpdatedAccount_ShouldUpdateExistingAccountCorrectly()
    {
        // Arrange
        var existingAccount = new AccountEntity
        {
            AccountId = 1,
            DeploymentEntity = null
        };

        var accountDetail = new AccountDetail
        {
            AccountNumber = "T12345",
            Phone = new List<Phone> { new Phone { PhoneNumber = "0600000000" } },
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Accounting = new Accounting
            {
                FiscalExerciseStartDate = new DateTime(2023, 1, 1),
                FiscalExerciseDuration = 12,
                AccountingType = "Type1",
                FiscalSystem = "System1",
                TaxationSystem = "Tax1",
                ActivityType = "Activity1",
                ActivityDescription = "Description1"
            },
            Deployment = new Deployment
            {
                DeploymentDate = new DateTime(2023, 6, 1),
                Status = 1
            },
            Hub = new Hub { HubId = 5 },
            Vat = new Vat { System = "VatSystem1", Type = "VatType1" }
        };

        // Act
        existingAccount.MapToUpdatedAccount(accountDetail);

        // Assert
        existingAccount.StaffSizeRange.Should().Be("10-50");
        existingAccount.FiscalExerciseStartDate.Should().Be(new DateTime(2023, 1, 1));
        existingAccount.FiscalExerciseDuration.Should().Be(12);
        existingAccount.AccountingMethod.Should().Be("Type1");
        existingAccount.FiscalSystem.Should().Be("System1");
        existingAccount.TaxationSystem.Should().Be("Tax1");
        existingAccount.ActivityType.Should().Be("Activity1");
        existingAccount.ActivityDescription.Should().Be("Description1");

        existingAccount.DeploymentEntity.Should().NotBeNull();
        var deployment = existingAccount.DeploymentEntity;
        deployment.AccountId.Should().Be(1);
        deployment.DeploymentDate.Should().Be(new DateTime(2023, 6, 1));
        deployment.Status.Should().Be(1);

        existingAccount.HubId.Should().Be(5);
        existingAccount.Vat.Should().Be("VatSystem1");
        existingAccount.Vattype.Should().Be("VatType1");
    }

    [Fact]
    public void MapToUpdatedAccount_WithNullProperties_ShouldNotUpdateCorrespondingFields()
    {
        // Arrange
        var existingAccount = new AccountEntity
        {
            AccountId = 1,
            StaffSizeRange = "Original",
            FiscalExerciseStartDate = new DateTime(2022, 1, 1),
            DeploymentEntity = null
        };

        var accountDetail = new AccountDetail
        {
            AccountNumber = "T12345",
            Phone = new List<Phone> { new Phone { PhoneNumber = "0600000000" } },
            Legal = null,
            Accounting = null,
            Deployment = null,
            Hub = null,
            Vat = null
        };

        // Act
        existingAccount.MapToUpdatedAccount(accountDetail);

        // Assert
        existingAccount.StaffSizeRange.Should().Be("Original");
        existingAccount.FiscalExerciseStartDate.Should().Be(new DateTime(2022, 1, 1));
        existingAccount.DeploymentEntity.Should().BeNull();
        existingAccount.HubId.Should().BeNull();
        existingAccount.Vat.Should().BeNull();
        existingAccount.Vattype.Should().BeNull();
    }

    [Fact]
    public void MapToUpdatedAccount_WithExistingDeployment_ShouldUpdateExistingDeployment()
    {
        // Arrange
        var existingAccount = new AccountEntity
        {
            AccountId = 1,
            DeploymentEntity = new DeploymentEntity
            {
                AccountId = 1,
                DeploymentDate = new DateTime(2022, 1, 1),
                Status = -1
            }
        };

        var accountDetail = new AccountDetail
        {
            AccountNumber = "T12345",
            Phone = new List<Phone> { new Phone { PhoneNumber = "0600000000" } },
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Deployment = new Deployment
            {
                DeploymentDate = new DateTime(2023, 6, 1),
                Status = 1
            }
        };

        // Act
        existingAccount.MapToUpdatedAccount(accountDetail);

        // Assert
        existingAccount.DeploymentEntity.Should().NotBeNull();
        var deployment = existingAccount.DeploymentEntity;
        deployment.AccountId.Should().Be(1);
        deployment.DeploymentDate.Should().Be(new DateTime(2023, 6, 1));
        deployment.Status.Should().Be(1);
    }
    [Fact]
    public void InitStatistic_WithValidData_ShouldReturnCorrectStatistics()
    {
        var countByAccountStatus = new Dictionary<int, int>
        {
            { 1, 10 },
            { 2, 20 },
            { 3, 30 },
            { 4, 40 }
        };

        var countByContactStatus = new Dictionary<string, int>
        {
            { ContactStatus.Connected.ToString(), 50 },
            { ContactStatus.Declared.ToString(), 60 },
            { ContactStatus.Invited.ToString(), 70 }
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.InitStatistic(countByAccountStatus, countByContactStatus);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(10);
        result.AccountInProgress.Should().Be(20);
        result.AccountConnected.Should().Be(30);
        result.AccountRevoked.Should().Be(40);
        result.ContactConnected.Should().Be(50);
        result.ContactDeclared.Should().Be(60);
        result.ContactInvited.Should().Be(70);
    }

    [Fact]
    public void InitStatistic_WithMissingData_ShouldReturnZeroForMissingValues()
    {
        // Arrange
        var countByAccountStatus = new Dictionary<int, int>
        {
            { 1, 10 },
            { 3, 30 }
        };

        var countByContactStatus = new Dictionary<string, int>
        {
            { ContactStatus.Connected.ToString(), 50 },
            { ContactStatus.Invited.ToString(), 70 }
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.InitStatistic(countByAccountStatus, countByContactStatus);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(10);
        result.AccountInProgress.Should().Be(0);
        result.AccountConnected.Should().Be(30);
        result.AccountRevoked.Should().Be(0);
        result.ContactConnected.Should().Be(50);
        result.ContactDeclared.Should().Be(0);
        result.ContactInvited.Should().Be(70);
    }

    [Fact]
    public void InitStatistic_WithNullDictionaries_ShouldReturnZeroForAllValues()
    {
        // Act
        var result = MapAccountDatabaseToAccountModel.InitStatistic(null, null);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(0);
        result.AccountInProgress.Should().Be(0);
        result.AccountConnected.Should().Be(0);
        result.AccountRevoked.Should().Be(0);
        result.ContactConnected.Should().Be(0);
        result.ContactDeclared.Should().Be(0);
        result.ContactInvited.Should().Be(0);
    }

    [Fact]
    public void InitStatistic_WithEmptyDictionaries_ShouldReturnZeroForAllValues()
    {
        // Arrange
        var countByAccountStatus = new Dictionary<int, int>();
        var countByContactStatus = new Dictionary<string, int>();

        // Act
        var result = MapAccountDatabaseToAccountModel.InitStatistic(countByAccountStatus, countByContactStatus);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(0);
        result.AccountInProgress.Should().Be(0);
        result.AccountConnected.Should().Be(0);
        result.AccountRevoked.Should().Be(0);
        result.ContactConnected.Should().Be(0);
        result.ContactDeclared.Should().Be(0);
        result.ContactInvited.Should().Be(0);
    }


    [Fact]
    public void UpdateDeploymentToEntity_WithNullDeploymentEntity_ShouldCreateNewDeploymentEntity()
    {
        // Arrange
        var deployment = new Deployment { Status = 1 };
        var account = new AccountEntity { DeploymentEntity = null };

        // Act
        deployment.UpdateDeploymentToEntity(account);

        // Assert
        account.DeploymentEntity.Should().NotBeNull();
        account.DeploymentEntity.Status.Should().Be(1);
        account.DeploymentEntity.DeploymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDeploymentToEntity_WithExistingDeploymentEntity_ShouldUpdateExistingEntity()
    {
        // Arrange
        var deployment = new Deployment { Status = 1 };
        var account = new AccountEntity
        {
            DeploymentEntity = new DeploymentEntity
            {
                Status = 1,
                DeploymentDate = DateTime.UtcNow.AddDays(-1)
            }
        };

        // Act
        deployment.UpdateDeploymentToEntity(account);

        // Assert
        account.DeploymentEntity.Should().NotBeNull();
        account.DeploymentEntity.Status.Should().Be(1);
        account.DeploymentEntity.DeploymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MapToAccounts_WithNullSource_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        ICollection<AccountEntity> source = null!;
        int? contactId = 1;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccounts(source, contactId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapToAccounts_WithValidSource_ShouldMapCorrectly()
    {
        // Arrange
        var source = new List<AccountEntity>
        {
            new AccountEntity { AccountId = 1, AccountNumber = "123" },
            new AccountEntity { AccountId = 2, AccountNumber = "456" }
        };
        int? contactId = 1;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccounts(source, contactId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.ElementAt(0).AccountId.Should().Be(1);
        result.ElementAt(1).AccountId.Should().Be(2);
    }

    [Fact]
    public void MapToAccount_WithNullSource_ShouldReturnNull()
    {
        // Arrange
        AccountEntity source = null;
        int? contactId = 1;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccount(source, contactId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MapToAccount_WithValidSource_ShouldMapCorrectly()
    {
        // Arrange
        var date = DateTime.Now;
        var source = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "123",
            LegalName = "Test Company",
            RoleEntity = new List<RoleEntity>
            {
                new RoleEntity
                {
                    IsSignatory = true,
                    Contact = new ContactEntity { Type = "Client" }
                },
                new RoleEntity
                {
                    ContactId = 1,
                    IsFavorite = true,
                    IsCustomerRelation = true,
                    LastActivityDate = date,
                }
            },
            AddressEntity = new List<AddressEntity>
            {
                new AddressEntity { AddressType = "Delivery" }
            },
            DeploymentEntity = new DeploymentEntity { Status = 1 },
            Hub = new HubEntity { HubId = 1, HubName = "Test Hub" }
        };

        int? contactId = 1;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccount(source, contactId);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(1);
        result.AccountGlobalUniqueId.Should().Be(source.AccountGlobalUniqueId);
        result.AccountNumber.Should().Be("123");
        result.LegalName.Should().Be("Test Company");
        result.Signatory.Should().NotBeNull();
        result.Address.Should().NotBeNull();
        result.Deployment.Should().NotBeNull();
        result.Hub.Should().NotBeNull();
        result.Hub.HubId.Should().Be(1);
        result.Hub.HubName.Should().Be("Test Hub");
        Assert.True(result.IsFavorite);
        Assert.True(result.IsCustomerRelation);
        result.LastActivityDate.Should().Be(date);
    }

    [Fact]
    public void MapToAccountSummary_WithValidSource_ShouldMapCorrectly()
    {
        // Arrange
        const int contactId = 1;
        var source = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "123",
            LegalName = "Test Company",
            AccountType = AccountType.PROSPECT.ToString(),
            MissionType = "Mission",
            RoleEntity = new List<RoleEntity>
            {
                new()
                {
                    ContactId = contactId,
                    IsSignatory = true
                }
            },
            AddressEntity = new List<AddressEntity>
            {
                new AddressEntity { AddressType = "Delivery" }
            },
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccountSummary(source, contactId);

        // Assert
        result.Should().NotBeNull();
        result!.AccountId.Should().Be(source.AccountId);
        result.AccountGlobalUniqueId.Should().Be(source.AccountGlobalUniqueId);
        result.AccountNumber.Should().Be(source.AccountNumber);
        result.LegalName.Should().Be(source.LegalName);
        result.AccountType.Should().Be(source.AccountType);
        result.MissionType.Should().Be(source.MissionType);
        result.Deployment.Should().NotBeNull();
        result.Address.Should().NotBeNull();
        result.IsSignatory.Should().BeTrue();
    }

    [Fact]
    public void MapToAccountDetail_WithNullSource_ShouldReturnNull()
    {
        // Arrange
        AccountEntity source = null!;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccountDetail(source);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MapToAccountDetail_WithValidSource_ShouldMapCorrectly()
    {
        // Arrange
        var source = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "123",
            IconName = "icon.png",
            IsActive = true,
            Email = "test@example.com",
            StaffSize = 50,
            CommercialName = "Test Corp",
            Vat = "VAT123",
            Vatintra = "VATINTRA123",
            Vattype = "TYPE1",
            AddressEntity = new List<AddressEntity>(),
            PhoneEntity = new List<PhoneEntity>(),
            Hub = new HubEntity { HubId = 1, HubName = "Test Hub" },
            DeploymentEntity = new DeploymentEntity { Status = 1 },
            Turnover = 1000000.50m
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToAccountDetail(source);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(1);
        result.AccountGlobalUniqueId.Should().Be(source.AccountGlobalUniqueId);
        result.AccountNumber.Should().Be("123");
        result.IconName.Should().Be("icon.png");
        result.IsActive.Should().BeTrue();
        result.Email.Should().Be("test@example.com");
        result.EmployeeCount.Should().Be(50);
        result.CommercialName.Should().Be("Test Corp");
        result.Vat.Should().NotBeNull();
        result.Vat.System.Should().Be("VAT123");
        result.Vat.Intra.Should().Be("VATINTRA123");
        result.Vat.Type.Should().Be("TYPE1");
        result.Address.Should().NotBeNull();
        result.Phone.Should().NotBeNull();
        result.Hub.Should().NotBeNull();
        result.Deployment.Should().NotBeNull();
        result.Turnover.Should().Be(1000000.50m);
    }

    [Fact]
    public void MapToStatistics_WithNullDictionaries_ShouldReturnZeroValues()
    {
        // Arrange
        Dictionary<int, int> countByAccountStatus = null!;
        Dictionary<string, int> countByContactStatus = null!;

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToStatistics(countByAccountStatus, countByContactStatus);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(0);
        result.AccountInProgress.Should().Be(0);
        result.AccountConnected.Should().Be(0);
        result.AccountRevoked.Should().Be(0);
        result.ContactConnected.Should().Be(0);
        result.ContactDeclared.Should().Be(0);
        result.ContactInvited.Should().Be(0);
    }

    [Fact]
    public void MapToNaf_ShouldMapCorrectly()
    {
        // Arrange
        var nafEntity = new NafEntity()
        {
            NafId = 1,
            NafCode = "code",
            NafLabel = "label"
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToNaf(nafEntity);

        // Assert
        result.Should().NotBeNull();
        result.NafId.Should().Be(1);
        result.NafCode.Should().Be("code");
        result.NafLabel.Should().Be("label");
    }

    [Fact]
    public void MapToStatistics_WithValidDictionaries_ShouldMapCorrectly()
    {
        // Arrange
        var countByAccountStatus = new Dictionary<int, int>
        {
            { 1, 10 },
            { 2, 20 },
            { 3, 30 },
            { 4, 40 }
        };
        var countByContactStatus = new Dictionary<string, int>
        {
            { ContactStatus.Connected.ToString(), 50 },
            { ContactStatus.Declared.ToString(), 60 },
            { ContactStatus.Invited.ToString(), 70 }
        };

        // Act
        var result = MapAccountDatabaseToAccountModel.MapToStatistics(countByAccountStatus, countByContactStatus);

        // Assert
        result.Should().NotBeNull();
        result.AccountToDeploy.Should().Be(10);
        result.AccountInProgress.Should().Be(20);
        result.AccountConnected.Should().Be(30);
        result.AccountRevoked.Should().Be(40);
        result.ContactConnected.Should().Be(50);
        result.ContactDeclared.Should().Be(60);
        result.ContactInvited.Should().Be(70);
    }

    [Theory]
    [InlineData(false, null, false)]
    [InlineData(true, "2024-01-01", false)]
    [InlineData(true, null, true)]
    public void MapToIsClarityVisible_ShouldReturnExpectedResult(
        bool isEligible,
        string? approvedDateString,
        bool expectedResult)
    {
        // Arrange
        DateTime? approvedDate = approvedDateString != null
            ? DateTime.Parse(approvedDateString)
            : (DateTime?)null;

        var account = new AccountEntity
        {
            OfferEligibilityEntity = new OfferEligibilityEntity
            {
                IsEligible = isEligible,
                ApprovedDate = approvedDate
            }
        };

        // Act
        var result = account.MapToIsClarityVisible();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void MapToIsClarityVisible_ReturnsFalse_WhenSourceIsNull()
    {
        // Arrange
        AccountEntity? account = null;

        // Act
        var result = account.MapToIsClarityVisible();

        // Assert
        result.Should().BeFalse();
    }

    // This interface is added to make the MapToAccounts method mockable
    public interface IAccountMapper
    {
        ICollection<Core.Models.Account> MapToAccounts(ICollection<AccountEntity> source, int? contactId);
    }

    [Fact]
    public void MapToActivatedOfferEligibility_Should_UpdateFields_WhenEntityNotNull()
    {
        // Arrange
        var entity = new OfferEligibilityEntity
        {
            AccountId = 123,
            IsEligible = true,
            ApprovedBy = null,
            ApprovedDate = null
        };
        var approvedBy = "admin@test.com";

        // Act
        entity.MapToActivatedOfferEligibility(approvedBy);

        // Assert
        entity.IsEligible.Should().BeFalse();
        entity.ApprovedBy.Should().Be(approvedBy);
        entity.ApprovedDate.Should().NotBeNull();
        entity.ApprovedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MapToActivatedOfferEligibility_Should_DoNothing_WhenEntityIsNull()
    {
        // Arrange
        OfferEligibilityEntity? entity = null;

        // Act
        entity.MapToActivatedOfferEligibility("ignored@test.com");

        // Assert
        entity.Should().BeNull();
    }

    [Fact]
    public void MapToOfferEligibility_Should_ReturnNull_WhenEntityIsNull()
    {
        // Arrange
        OfferEligibilityEntity? entity = null;

        // Act
        var result = entity.MapToOfferEligibility();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MapToOfferEligibility_Should_MapPropertiesCorrectly()
    {
        // Arrange
        var entity = new OfferEligibilityEntity
        {
            AccountId = 456,
            OfferName = "name",
            ApprovedBy = "admin@test.com",
            ApprovedDate = new DateTime(2024, 12, 15),
            IsEligible = true,
            ReportId = 1,
            ReportLabel = "reportlabel"
        };

        // Act
        var result = entity.MapToOfferEligibility();

        // Assert
        result.Should().NotBeNull();
        result!.AccountId.Should().Be(entity.AccountId);
        result.ApprovedBy.Should().Be(entity.ApprovedBy);
        result.ApprovedDate.Should().Be(entity.ApprovedDate);
        result.IsEligible.Should().Be(entity.IsEligible);
        result.OfferName.Should().Be(entity.OfferName);
        result.Reporting!.ReportId.Should().Be(entity.ReportId);
        result.Reporting.ReportLabel.Should().Be(entity.ReportLabel);
    }
}
