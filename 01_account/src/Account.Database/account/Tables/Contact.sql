CREATE TABLE [actor].[Contact]
(
	[ContactId]				INT	                NOT NULL,
	[ContactGlobalUniqueId]	UNIQUEIDENTIFIER	NULL,
	[FirstName]				VARCHAR(100)		NOT NULL,
	[LastName]				VARCHAR(100)		NOT NULL,
	[Email]     			VARCHAR(255)		NOT NULL,
    [LandPhone]             VARCHAR(255)        NULL,
	[MobilePhone]           VARCHAR(255)        NULL,
	[Type]                  VARCHAR(20)         NOT NULL, 
	[Status]                VARCHAR(20)         NULL, 
	[PersonaName]           VARCHAR(50)         NOT NULL,
	[CreationDate]          DATETIME2           NOT NULL DEFAULT GETDATE(),
    [LastUpdateDate]        DATETIME2           NULL, 
    [IsActive]              BIT                 NOT NULL DEFAULT (1),
    [OfficeId]              INT                 NULL,
    CONSTRAINT [C_Contact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC),
    CONSTRAINT [C_Contact_OfficeId_FK] FOREIGN KEY ([OfficeId]) REFERENCES [account].[Office] ([OfficeId])
)

GO
CREATE NONCLUSTERED INDEX [IX_Contact_Email_LastName_FirstName_IsActive]
    ON  [actor].[Contact]([Email], [LastName], [FirstName], [IsActive]);
GO
CREATE NONCLUSTERED INDEX [IX_Contact_Type_IsActive]
    ON  [actor].[Contact]([Type], [IsActive]);
GO
CREATE NONCLUSTERED INDEX [IX_Contact_Status_IsActive]
    ON  [actor].[Contact]([Status], [IsActive]);
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
    @value = N'Le numéro de téléphone du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'LandPhone'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le numéro de téléphone portable du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'MobilePhone'
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
    @value = N'L''identifiant du bureau',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'OfficeId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'CreationDate'
    GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de la dernière modification du contact',
    @level0type = N'SCHEMA',
    @level0name = N'actor',
    @level1type = N'TABLE',
    @level1name = N'Contact',
    @level2type = N'COLUMN',
    @level2name = N'LastUpdateDate'
