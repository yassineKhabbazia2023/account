CREATE TABLE [account].[DelegationDetail]
(
	[DelegationId]			INT NOT NULL,
	[AccountId]				INT NOT NULL,
	[DelegateeId]			INT NOT NULL,
	CONSTRAINT [C_DelegationDetail_PK] PRIMARY KEY CLUSTERED (DelegationId, AccountId, DelegateeId)
)

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
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du délégataire',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationDetail',
    @level2type = N'COLUMN',
    @level2name = N'DelegateeId'
GO