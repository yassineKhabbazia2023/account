CREATE TABLE [actor].[Contact]
(
	[ContactId]				INT IDENTITY(1, 1)	NOT NULL,
	[ContactGlobalUniqueId]	UNIQUEIDENTIFIER	NOT NULL,
	[FirstName]				VARCHAR(50)			NOT NULL,
	[LastName]				VARCHAR(50)			NOT NULL,
	[Email]     			VARCHAR(50)			NOT NULL,
	[Type]                  VARCHAR(20)         NOT NULL, 
	[Status]                VARCHAR(20)         NOT NULL, 
	[PersonaName]           VARCHAR(50)         NOT NULL, 
	[CreationDate]          DATETIME2           NOT NULL, 
    CONSTRAINT [C_Contact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC),
    CONSTRAINT [CHK_Type] CHECK ([Type]= 'Client' OR [Type]= 'Collaborator'),
    CONSTRAINT [CHK_Status] CHECK ([Status]= 'Connected' OR [Status]= 'Declared' OR [Status]= 'Invited')
)

GO
CREATE NONCLUSTERED INDEX [IDX_Contact_ContactGlobalUniqueId]
    ON  [actor].[Contact]([ContactGlobalUniqueId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'ContactId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant global du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'ContactGlobalUniqueId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le prénom du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'FirstName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nom du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'LastName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''adresse mail du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'Email'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type de contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'Type'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le statut de contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'Status'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nom du persona',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'PersonaName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'CreationDate'
