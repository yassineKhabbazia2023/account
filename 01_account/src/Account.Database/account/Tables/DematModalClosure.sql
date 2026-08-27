CREATE TABLE [account].[DematModalClosure]
(
    [AccountId]  INT           NOT NULL,
    [ContactId]  INT           NOT NULL,
    [ClosedDate] DATETIME2 (7) NOT NULL,
    CONSTRAINT [C_DematModalClosure_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC, [ContactId] ASC),
    CONSTRAINT [C_DematModalClosure_Account_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
    CONSTRAINT [C_DematModalClosure_Contact_FK] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique du compte, clé primaire composite avec ContactId (une fermeture par contact et par compte)',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DematModalClosure',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du contact ayant fermé le modal, clé primaire composite avec AccountId (chaque contact ferme son propre modal, indépendamment des autres contacts du compte)',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DematModalClosure',
    @level2type = N'COLUMN',
    @level2name = N'ContactId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de fermeture du modal',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DematModalClosure',
    @level2type = N'COLUMN',
    @level2name = N'ClosedDate'
GO
