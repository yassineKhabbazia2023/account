CREATE TABLE [account].[DelegationDetail]
(
	[DelegationId]			INT NOT NULL,
	[AccountId]				INT NOT NULL,
	CONSTRAINT [C_DelegationDetail_PK] PRIMARY KEY CLUSTERED (DelegationId, AccountId),
    CONSTRAINT [C_DelegationDetail_Delegation_FK] FOREIGN KEY ([DelegationId]) REFERENCES [account].[Delegation] ([DelegationId]),
    CONSTRAINT [C_DelegationDetail_Account_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_DelegationDetail_AccountId]
    ON  [account].[DelegationDetail]([AccountId] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de la délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationDetail',
    @level2type = N'COLUMN',
    @level2name = N'DelegationId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationDetail',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO