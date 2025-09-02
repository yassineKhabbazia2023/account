CREATE TABLE [account].[OfferEligibility]
(
    [AccountId]   INT           NOT NULL,
    [OfferName]   VARCHAR(100)  NOT NULL,
    [IsEligible]  BIT           NOT NULL,
    [ApprovedDate] DATETIME2    NOT NULL,
    [ApprovedBy]  VARCHAR(255)  NOT NULL,
    CONSTRAINT [C_AccountId_PK] 
        PRIMARY KEY CLUSTERED ([AccountId] ASC),
    CONSTRAINT [C_Account_OfferEligibility_FK] 
        FOREIGN KEY ([AccountId]) REFERENCES [account].[Account]([AccountId])
);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Identifiant technique du compte (clé primaire et étrangère vers account.Account)',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'OfferEligibility',
    @level2type = N'COLUMN', @level2name = N'AccountId';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Nom de l''offre associée à ce compte',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'OfferEligibility',
    @level2type = N'COLUMN', @level2name = N'OfferName';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Indique si le compte est éligible à l''offre (1 = Oui, 0 = Non)',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'OfferEligibility',
    @level2type = N'COLUMN', @level2name = N'IsEligible';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Date de validation de l''offre',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'OfferEligibility',
    @level2type = N'COLUMN', @level2name = N'ApprovedDate';
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Utilisateur ou processus ayant validé l''offre',
    @level0type = N'SCHEMA', @level0name = N'account',
    @level1type = N'TABLE',  @level1name = N'OfferEligibility',
    @level2type = N'COLUMN', @level2name = N'ApprovedBy';
GO
