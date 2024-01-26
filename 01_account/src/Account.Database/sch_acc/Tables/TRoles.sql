CREATE TABLE [sch_acc].[TRoles]
(
	[RoleId]			INT IDENTITY(1, 1)	NOT NULL,
	[AccountId]			INT					NOT NULL,
	[ContactId]			INT					NOT NULL,
	[HasBeenDelegated]   BIT					NULL,
	[IsFavorite]		BIT					NULL,
	[IsTemporary]		BIT					NULL,
	[EndDate]           DATETIME2			NULL,
	CONSTRAINT [C_TRoles_PK] PRIMARY KEY CLUSTERED ([RoleId] ASC),
	CONSTRAINT [C_TAccount_TRoles_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId]),
	CONSTRAINT [C_TAccount_TContact_FK] FOREIGN KEY ([ContactId]) REFERENCES [sch_acc].[TContact] ([ContactId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TRoles_AccountId]
    ON [sch_acc].[TRoles]([AccountId] ASC);

GO
CREATE NONCLUSTERED INDEX [IDX_TRoles_ContactId]
    ON [sch_acc].[TRoles]([ContactId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'RoleId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du contact',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'ContactId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le rôle est-il confié ou attribué à quelqu''un d''autre',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'HasBeenDelegated'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le rôle est-il considéré comme un favori ou mis en avant comme tel',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'IsFavorite'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le rôle est-il exercé pour une durée limitée',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'IsTemporary'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de fin ou de validité du rôle',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TRoles',
    @level2type = N'COLUMN',
    @level2name = N'EndDate'