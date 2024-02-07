CREATE TABLE [sch_acc].[TAccount] (
[AccountId]                 INT IDENTITY(1,1) NOT NULL,
[AccountGlobalUniqueId]     UNIQUEIDENTIFIER  NOT NULL,
[LegalName]                 NVARCHAR(255)     NOT NULL,
[CommercialName]            NVARCHAR(255)     NULL,
[AccountType]               VARCHAR(50)       NULL, 
[Email]                     NVARCHAR(100)     NOT NULL,
[HubId]                     INT               NULL,
[NafId]                     INT               NOT NULL,
[IsActive]                  BIT               NOT NULL,
[SourceAccountNumber]       VARCHAR(50)       NOT NULL,
[SectorCode]                VARCHAR(50)       NULL,
[Sector]                    VARCHAR(150)      NULL,
[StaffSizeRange]            VARCHAR(50)       NULL,
[AccountingMethod]          VARCHAR(150)      NULL,
[Turnover]                  DECIMAL           NULL,
[LegalFormCode]             VARCHAR(150)      NULL,
[LegalForm]                 VARCHAR(150)      NULL,
[FiscalSystem]              VARCHAR(50)       NULL,
[FiscalExerciseStartDate]   DATETIME2         NULL,
[FiscalExerciseDuration]    INT               NULL,
[ISIN]                      VARCHAR(150)      NULL,
[Siret]                     VARCHAR(150)      NULL,
[TaxationSystem]            VARCHAR(150)      NULL,
[ActivityDescription]       VARCHAR(150)      NULL,
[ActivityType]              VARCHAR(150)      NULL,
[VAT]                       VARCHAR(150)      NULL,
[VATIntra]                  VARCHAR(20)       NULL,
[VATType]                   VARCHAR(20)       NULL,
[StaffSize]                 INT               NULL,
[CreatedBy]                 VARCHAR(50)       NOT NULL,
[ModifiedBy]                VARCHAR(50)       NULL,
[CreateDate]                DATETIME2         NOT NULL,
[UpdateDate]                DATETIME2         NULL,
CONSTRAINT [C_TAccount_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC),
CONSTRAINT [UQ_TAccount_AccountGlobalUniqueId] UNIQUE NONCLUSTERED ([AccountGlobalUniqueId] ASC),
CONSTRAINT [UQ_TPhone_PhoneId] UNIQUE NONCLUSTERED ([AccountGlobalUniqueId] ASC),
CONSTRAINT [C_TAccount_TNafId_FK] FOREIGN KEY ([NafId]) REFERENCES [sch_acc].[TNaf] ([NafId]),
CONSTRAINT [C_TAccount_THub_HubId_FK] FOREIGN KEY ([HubId]) REFERENCES [sch_acc].[THub] ([HubId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TAccount_AccountGlobalUniqueId]
    ON [sch_acc].[TAccount]([AccountGlobalUniqueId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_THub_HubId]
    ON  [sch_acc].[TAccount]([HubId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_TNaf_NafId]
    ON  [sch_acc].[TAccount]([NafId] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de la dernière modification',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = 'UpdateDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de la création de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = 'CreateDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant de l''utilisateur ou du système qui a effectué la dernière modification',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'ModifiedBy'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant de l''utilisateur ou du système qui a crée l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'CreatedBy'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nombre d''employés de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'StaffSize'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Type de TVA',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'VATType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le numéro de TVA intracommunautaire',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = 'VATIntra'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Description de l''activité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'ActivityDescription'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Type d''activité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'ActivityType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La TVA',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'VAT'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le  nom commercial de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'CommercialName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La raison social de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'LegalName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'AccountType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le Régime d''imposition',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'TaxationSystem'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le Siret',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'Siret'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le ISIN',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'ISIN'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La durée de l''exercice fiscale',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = 'FiscalExerciseDuration'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Début Exercice fiscale',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = 'FiscalExerciseStartDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type de comptabilité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'AccountingMethod'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le secteur',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'Sector'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code du secteur',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'SectorCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La source de création de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'SourceAccountNumber'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''entité est-elle activé',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'IsActive'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''adresse mail de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'Email'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code de la forme juridique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'LegalFormCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La forme juridique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'LegalForm'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La plage du nombre de salariés ',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'StaffSizeRange'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le chiffre d''affaires',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'Turnover'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant global de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'AccountGlobalUniqueId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du Hub',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'HubId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du code Naf',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAccount',
    @level2type = N'COLUMN',
    @level2name = N'NafId'