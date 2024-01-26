CREATE TABLE [sch_acc].[TDelegation]
(
	[DelegationId]          INT IDENTITY(1,1)	NOT NULL,
	[AccountId]				INT 				NOT NULL,
	[DelegatorId]		    INT 				NOT NULL,
	[DelegateeId]	        INT 				NOT NULL,
	[StartDate]				DATETIME2			NOT NULL,
	[EndDate]				DATETIME2			NULL,
	[Status]				INT                 NOT NULL,
    [Note]                  VARCHAR(255)        NULL,
	[CreationDate]			DATETIME2			NOT NULL, 
	CONSTRAINT [C_TDelegation_PK] PRIMARY KEY CLUSTERED ([DelegationId] ASC),
	CONSTRAINT [C_TDelegation_TAccount_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId]),
	CONSTRAINT [C_TDelegation_TContact_DelegatorId_FK] FOREIGN KEY ([DelegatorId]) REFERENCES [sch_acc].[TContact] ([ContactId]),
	CONSTRAINT [C_TDelegation_TContact_DelegateeId_FK] FOREIGN KEY ([DelegateeId]) REFERENCES [sch_acc].[TContact] ([ContactId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_AccountId]
    ON  [sch_acc].[TDelegation]([AccountId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_DelegatorId]
    ON  [sch_acc].[TDelegation]([DelegatorId] ASC)

GO
CREATE NONCLUSTERED INDEX [IDX_TDelegation_DelegateeId]
    ON  [sch_acc].[TDelegation]([DelegateeId] ASC)
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
    @value = N'La délégation est-elle active ou non',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = 'Status'
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
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le délégateur ',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegatorId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le délégataire',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegateeId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La note associé à la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDelegation',
    @level2type = N'COLUMN',
    @level2name = N'Note'