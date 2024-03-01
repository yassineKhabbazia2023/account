CREATE TABLE [account].[Role]
(
	[RoleId]			INT IDENTITY(1, 1)	NOT NULL,
	[AccountId]			INT					NOT NULL,
	[ContactId]			INT					NOT NULL,
	[IsFavorite]		BIT					NULL,
	[IsSignatory]		BIT					NULL,
	CONSTRAINT [C_Role_PK] PRIMARY KEY CLUSTERED ([RoleId] ASC),
	CONSTRAINT [C_Account_Role_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
	CONSTRAINT [C_Account_Contact_FK] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId]), 
    CONSTRAINT [C_Role_AccountId_ContactId] UNIQUE ([AccountId], [ContactId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_Role_AccountId]
    ON [account].[Role]([AccountId] ASC);

GO
CREATE NONCLUSTERED INDEX [IDX_Role_ContactId]
    ON [account].[Role]([ContactId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Role',
    @level2type = N'COLUMN',
    @level2name = N'RoleId'
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
