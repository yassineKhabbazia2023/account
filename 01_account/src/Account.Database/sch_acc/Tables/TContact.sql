CREATE TABLE [sch_acc].[TContact]
(
	[ContactId]				INT IDENTITY(1, 1)	NOT NULL,
	[ContactGlobalUniqueId]	UNIQUEIDENTIFIER	NOT NULL,
	[FirstName]				VARCHAR(50)			NOT NULL,
	[LastName]				VARCHAR(50)			NOT NULL,
	[ContactEmail]			VARCHAR(50)			NOT NULL,
	CONSTRAINT [C_TContact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC)	
)

GO
CREATE NONCLUSTERED INDEX [IDX_TContact_ContactGlobalUniqueId]
    ON  [sch_acc].[TContact]([ContactGlobalUniqueId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TContact',
    @level2type = N'COLUMN',
    @level2name = N'ContactId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant global du contact',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TContact',
    @level2type = N'COLUMN',
    @level2name = N'ContactGlobalUniqueId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le prénom du contact',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TContact',
    @level2type = N'COLUMN',
    @level2name = N'FirstName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nom du contact',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TContact',
    @level2type = N'COLUMN',
    @level2name = N'LastName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'l''adresse mail du contact',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TContact',
    @level2type = N'COLUMN',
    @level2name = N'ContactEmail'