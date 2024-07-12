CREATE TABLE [account].[Role]
(
	[AccountId]			INT					NOT NULL,
	[ContactId]			INT					NOT NULL,
	[IsFavorite]		BIT					NULL,
	[IsSignatory]		BIT					NULL,
    [IsDelegation]      BIT                 NULL,
	CONSTRAINT [C_Role_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC, [AccountId] ASC),
	CONSTRAINT [C_Account_Role_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
	CONSTRAINT [C_Account_Contact_FK] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId]), 
    CONSTRAINT [C_Role_AccountId_ContactId] UNIQUE ([AccountId], [ContactId])
)

GO
CREATE NONCLUSTERED INDEX [IX_Role_IsFavorite]
    ON [account].[Role]([IsFavorite] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Role_IsSignatory]
    ON [account].[Role]([IsSignatory] ASC);

GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du contact',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = N'ContactId'
GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le rôle est-il considéré comme un favori ou mis en avant comme tel',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = N'IsFavorite'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le signataire',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = 'IsSignatory'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Indique, dans les cas où c''est possible, si le role est lié à une délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = 'IsDelegation'
GO