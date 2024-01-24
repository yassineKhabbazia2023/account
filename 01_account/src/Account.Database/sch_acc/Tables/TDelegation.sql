CREATE TABLE [sch_acc].[TDelegation]
(
	[DelegationId]          INT IDENTITY(1,1)	NOT NULL,
	[AccountId]				INT 				NOT NULL,
	[ContactSourceId]		INT 				NOT NULL,
	[ContactDestinationId]	INT 				NOT NULL,
	[IsEnable]				BIT                 NOT NULL,
	[StartDate]				DATETIME2			NOT NULL,
	[EndDate]				DATETIME2			NOT NULL,
	[CreationDate]			DATETIME2			NOT NULL, 
	CONSTRAINT [C_TDelegation_PK] PRIMARY KEY CLUSTERED ([DelegationId] ASC),
	CONSTRAINT [C_TDelegation_TAccount_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId]),
	CONSTRAINT [C_TDelegation_TContact_ContactSourceId_FK] FOREIGN KEY ([ContactSourceId]) REFERENCES [sch_acc].[TContact] ([ContactId]),
	CONSTRAINT [C_TDelegation_TContact_ContactDestinationId_FK] FOREIGN KEY ([ContactDestinationId]) REFERENCES [sch_acc].[TContact] ([ContactId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_AccountId]
    ON  [sch_acc].[TDelegation]([AccountId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_ContactSourceId]
    ON  [sch_acc].[TDelegation]([ContactSourceId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_ContactDestinationId]
    ON  [sch_acc].[TDelegation]([ContactDestinationId] ASC)
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegationId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du contact gestionnaire de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'ContactSourceId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du contact à qui est déléguée la gestion de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'ContactDestinationId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La délégation est-elle active ou non',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'IsEnable'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date effective du début de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'StartDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date effective de la fin de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'EndDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'CreationDate'