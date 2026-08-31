CREATE TABLE [account].[SerenityChoice]
(
    [ContactId]  INT       NOT NULL,
    [IsAccepted] BIT       NOT NULL,
    [ChoiceDate] DATETIME2 NOT NULL,
    CONSTRAINT [C_SerenityChoice_PK]
        PRIMARY KEY CLUSTERED ([ContactId] ASC),
    CONSTRAINT [C_SerenityChoice_Contact_FK]
        FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Identifiant technique du contact ayant fait le choix (clé primaire et étrangère vers actor.Contact). La présence de la ligne vaut "un choix a été fait".',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'SerenityChoice',
    @level2type = N'COLUMN', @level2name = N'ContactId';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Choix exprimé par le contact sur la modal Sérénité (1 = accepté, 0 = refusé). Conservé pour le métier : la décision d''affichage ne dépend que de l''existence de la ligne.',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'SerenityChoice',
    @level2type = N'COLUMN', @level2name = N'IsAccepted';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Date UTC à laquelle le choix a été enregistré',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'SerenityChoice',
    @level2type = N'COLUMN', @level2name = N'ChoiceDate';
GO
