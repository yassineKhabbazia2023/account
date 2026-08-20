CREATE TABLE [account].[Account] (
    [AccountId]               INT              IDENTITY (1, 1) NOT NULL,
    [AccountGlobalUniqueId]   UNIQUEIDENTIFIER NOT NULL,
    [AccountNumber]           VARCHAR (20)    NOT NULL,
    [LegalName]               NVARCHAR (255)   NOT NULL,
    [CommercialName]          NVARCHAR (255)   NULL,
    [AccountType]             VARCHAR (50)     NULL,
    [Email]                   NVARCHAR (255)   NULL,
    [DeliveryEmail]           NVARCHAR (255)   NULL,
    [BillingEmail]            NVARCHAR (255)   NULL,
    [DeliveryFax]             NVARCHAR (50)    NULL,
    [BillingFax]              NVARCHAR (50)    NULL,
    [HubId]                   INT              NULL,
    [NafId]                   INT              NULL,
    [IsActive]                BIT              NOT NULL,
    [SectorCode]              VARCHAR (50)     NULL,
    [Sector]                  VARCHAR (150)    NULL,
    [StaffSizeRange]          VARCHAR (50)     NULL,
    [AccountingMethod]        VARCHAR (150)    NULL,
    [Turnover]                DECIMAL (18, 2)  NULL,
    [LegalFormCode]           VARCHAR (150)    NULL,
    [LegalForm]               VARCHAR (150)    NULL,
    [FiscalSystem]            VARCHAR (50)     NULL,
    [FiscalExerciseStartDate] DATETIME2 (7)    NULL,
    [FiscalExerciseDuration]  INT              NULL,
    [ISIN]                    VARCHAR (150)    NULL,
    [Siret]                   VARCHAR (14)    NULL,
    [TaxationSystem]          VARCHAR (150)    NULL,
    [ActivityDescription]     VARCHAR (150)    NULL,
    [ActivityType]            VARCHAR (150)    NULL,
    [VAT]                     VARCHAR (50)     NULL,
    [VATIntra]                VARCHAR (20)     NULL,
    [VATType]                 VARCHAR (20)     NULL,
    [StaffSize]               INT              NULL,
    [SourceName]              VARCHAR (50)     NULL,
    [CreatedBy]               VARCHAR (50)     NOT NULL,
    [ModifiedBy]              VARCHAR (50)     NULL,
    [CreationDate]            DATETIME2 (7)    NOT NULL,
    [UpdatedDate]             DATETIME2 (7)    NULL,
    [IconName]                VARCHAR (50)     NULL,
    [MissionType]             VARCHAR (150)    NULL,
    [OfficeId]                INT              NULL,
    -- Provisional NVARCHAR(255) until Akuiteo confirms the definitive field types and lengths.
    [AccountRoutingCode]          NVARCHAR (255) NULL,
    [AccountRoutingLabel]         NVARCHAR (255) NULL,
    [AccountLegalFormLabel]       NVARCHAR (255) NULL,
    [AccountElectronicAddressId]  NVARCHAR (MAX) NULL,
    CONSTRAINT [C_Account_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC),
    CONSTRAINT [C_Account_Hub_HubId_FK] FOREIGN KEY ([HubId]) REFERENCES [account].[Hub] ([HubId]),
    CONSTRAINT [C_Account_NafId_FK] FOREIGN KEY ([NafId]) REFERENCES [account].[Naf] ([NafId]),
    CONSTRAINT [C_Account_OfficeId_FK] FOREIGN KEY ([OfficeId]) REFERENCES [account].[Office] ([OfficeId]),
    CONSTRAINT [UQ_Account_AccountGlobalUniqueId] UNIQUE NONCLUSTERED ([AccountGlobalUniqueId] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_Account_AccountNumber]
    ON  [account].[Account]([AccountNumber] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_Account_LegalName]
    ON  [account].[Account]([LegalName] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_Hub_HubId]
    ON  [account].[Account]([HubId] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_Naf_NafId]
    ON  [account].[Account]([NafId] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_Office_OfficeId]
    ON  [account].[Account]([OfficeId] ASC)

GO
CREATE NONCLUSTERED INDEX [IX_Account_MissionType]
    ON  [account].[Account]([MissionType] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant de l''utilisateur ou du système qui a effectué la dernière modification',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'ModifiedBy'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant de l''utilisateur ou du système qui a crée l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'CreatedBy'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nombre d''employés de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'StaffSize'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Type de TVA',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'VATType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le numéro de TVA intracommunautaire',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = 'VATIntra'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Description de l''activité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'ActivityDescription'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Type d''activité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'ActivityType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La TVA',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'VAT'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le  nom commercial de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'CommercialName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La raison social de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'LegalName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = 'AccountType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le Régime d''imposition',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'TaxationSystem'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le Siret',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'Siret'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le ISIN',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'ISIN'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La durée de l''exercice fiscale',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = 'FiscalExerciseDuration'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Début Exercice fiscale',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = 'FiscalExerciseStartDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type de comptabilité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountingMethod'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le secteur',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'Sector'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code du secteur',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'SectorCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''entité est-elle activé',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'IsActive'
GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''adresse mail de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'Email'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code de la forme juridique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'LegalFormCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La forme juridique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'LegalForm'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La plage du nombre de salariés ',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'StaffSizeRange'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le chiffre d''affaires',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'Turnover'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant global de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountGlobalUniqueId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du Hub',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'HubId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du code Naf',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'NafId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le régime fiscale',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'FiscalSystem'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Code de routage Akuiteo. Type et longueur NVARCHAR(255) provisoires en attente du contrat définitif.',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountRoutingCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Libellé de routage Akuiteo. Type et longueur NVARCHAR(255) provisoires en attente du contrat définitif.',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountRoutingLabel'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Libellé de forme juridique Akuiteo. Type et longueur NVARCHAR(255) provisoires en attente du contrat définitif.',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountLegalFormLabel'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Identifiant d’adresse électronique Akuiteo. Liste des sites écrasée dans une seule chaîne : une ligne par site (nom d’appel, code, SIRET, identifiant d’adressage séparés par "-"), lignes séparées par "/". Type NVARCHAR(MAX) pour ne pas contraindre la taille de cette liste.',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountElectronicAddressId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'CreationDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de la dernière modification',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'UpdatedDate'
