/*
==============================================================================
Script de migration : Création de la table SerenityChoice
Date : 2026-08-19
Description : Création idempotente de la table de persistance du choix Sérénité
              (un choix par contact) et de l'index filtré servant la requête
              d'éligibilité sur le portefeuille.
Prérequis  : Doit être exécuté AVANT le déploiement de l'API.
==============================================================================
*/

-- Vérifier si la table existe déjà
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[account].[SerenityChoice]') AND type in (N'U'))
BEGIN
    PRINT 'Création de la table [account].[SerenityChoice]...'

    CREATE TABLE [account].[SerenityChoice]
    (
        [ContactId]  INT       NOT NULL,
        [IsAccepted] BIT       NOT NULL,
        [ChoiceDate] DATETIME2 NOT NULL,
        CONSTRAINT [C_SerenityChoice_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC),
        CONSTRAINT [C_SerenityChoice_Contact_FK] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
    )

    PRINT 'Table [account].[SerenityChoice] créée avec succès.'
END
ELSE
BEGIN
    PRINT 'La table [account].[SerenityChoice] existe déjà, aucune action.'
END
GO

-- Index filtré servant la requête d'éligibilité Sérénité
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Account_AccountRoutingCode' AND object_id = OBJECT_ID(N'[account].[Account]'))
BEGIN
    PRINT 'Création de l''index [IX_Account_AccountRoutingCode]...'

    CREATE NONCLUSTERED INDEX [IX_Account_AccountRoutingCode]
        ON [account].[Account]([AccountRoutingCode] ASC)
        INCLUDE ([AccountElectronicAddressId])
        WHERE [AccountRoutingCode] IS NOT NULL

    PRINT 'Index [IX_Account_AccountRoutingCode] créé avec succès.'
END
ELSE
BEGIN
    PRINT 'L''index [IX_Account_AccountRoutingCode] existe déjà, aucune action.'
END
GO
