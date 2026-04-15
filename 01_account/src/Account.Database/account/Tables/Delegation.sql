CREATE TABLE [account].[Delegation]
(
	[DelegationId]              INT IDENTITY(1,1)	NOT NULL,
	[DelegatorId]		        INT 				NOT NULL,
    [DelegateeId]			    INT                 NOT NULL,
	[StartDate]				    DATETIME2			NOT NULL,
	[EndDate]				    DATETIME2			NULL,
	[Status]				    VARCHAR(10)         NOT NULL,
    [Note]                      VARCHAR(255)        NULL,
	[CreationDate]			    DATETIME2			NOT NULL,
    [IsFullDelegation]          BIT                 NOT NULL DEFAULT 0,
    [IsAutomaticDelegation]     BIT                 NOT NULL DEFAULT 0,
    [IncludePennylaneAccess]    BIT                 NOT NULL DEFAULT 0,
	CONSTRAINT [C_Delegation_PK] PRIMARY KEY CLUSTERED ([DelegationId] ASC),
	CONSTRAINT [C_Delegation_Contact_DelegatorId_FK] FOREIGN KEY ([DelegatorId]) REFERENCES actor.[Contact] ([ContactId]),
    CONSTRAINT [C_DelegationDetail_Contact_FK] FOREIGN KEY ([DelegateeId]) REFERENCES [actor].[Contact] ([ContactId]),
    CONSTRAINT [CHK_Status] CHECK ([Status] = 'pending' OR [Status] = 'enabled' OR [Status] = 'disabled')
)

GO
CREATE NONCLUSTERED INDEX [IDX_Delegation_DelegatorId]
    ON  [account].[Delegation]([DelegatorId] ASC)
GO
CREATE NONCLUSTERED INDEX [IDX_Delegation_DelegatorId_Status_IsAutomatic]
    ON [account].[Delegation]([DelegatorId] ASC, [Status] ASC, [IsAutomaticDelegation] ASC)
    INCLUDE ([CreationDate], [DelegateeId])
GO
CREATE NONCLUSTERED INDEX [IDX_Delegation_DelegateeId]
    ON  [account].[Delegation]([DelegateeId] ASC)
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegationId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La délégation est-elle active ou non',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = 'Status'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date effective du début de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'StartDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date effective de la fin de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'EndDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'CreationDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le délégateur ',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegatorId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du délégataire',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'DelegateeId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La note associé à la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'Note'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Indique si la délégation concerne l''intégralité du portefeuille ou non',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'IsFullDelegation'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Indique s''il s''agit d''une délégation automatique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'IsAutomaticDelegation'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Indique si l''accès à Pennylane doit être inclus.',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Delegation',
    @level2type = N'COLUMN',
    @level2name = N'IncludePennylaneAccess'
GO