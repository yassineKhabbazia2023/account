/*
==============================================================================
Script de migration : Création de la table DelegationRequest
Date : 2026-05-28
Description : Création idempotente de la table pour éviter la perte de données
==============================================================================
*/

-- Vérifier si la table existe déjà
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[account].[DelegationRequest]') AND type in (N'U'))
BEGIN
    PRINT 'Création de la table [account].[DelegationRequest]...'
    
    CREATE TABLE [account].[DelegationRequest]
    (
        [DelegationRequestId]   INT IDENTITY(1,1)   NOT NULL,
        [RequesterId]           INT                 NOT NULL,
        [RecipientId]           INT                 NOT NULL,
        [AccountId]             INT                 NOT NULL,
        [CreatedAt]             DATETIME2           NOT NULL,
        [Status]                VARCHAR(10)         NOT NULL,
        [RespondedAt]           DATETIME2           NULL,
        CONSTRAINT [C_DelegationRequest_PK] PRIMARY KEY CLUSTERED ([DelegationRequestId] ASC),
        CONSTRAINT [C_DelegationRequest_Requester_FK] FOREIGN KEY ([RequesterId]) REFERENCES [actor].[Contact] ([ContactId]),
        CONSTRAINT [C_DelegationRequest_Recipient_FK] FOREIGN KEY ([RecipientId]) REFERENCES [actor].[Contact] ([ContactId]),
        CONSTRAINT [C_DelegationRequest_Account_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
        CONSTRAINT [CHK_DelegationRequest_Status] CHECK ([Status] IN ('pending', 'accepted', 'refused'))
    )
    
    PRINT 'Table [account].[DelegationRequest] créée avec succès.'
END
ELSE
BEGIN
    PRINT 'La table [account].[DelegationRequest] existe déjà. Aucune action nécessaire.'
END
GO

-- Créer les index s'ils n'existent pas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DelegationRequest_RecipientId_Status' 
               AND object_id = OBJECT_ID(N'[account].[DelegationRequest]'))
BEGIN
    PRINT 'Création de l''index IX_DelegationRequest_RecipientId_Status...'
    CREATE NONCLUSTERED INDEX [IX_DelegationRequest_RecipientId_Status]
        ON [account].[DelegationRequest]([RecipientId] ASC, [Status] ASC);
    PRINT 'Index IX_DelegationRequest_RecipientId_Status créé.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DelegationRequest_RequesterId_Status' 
               AND object_id = OBJECT_ID(N'[account].[DelegationRequest]'))
BEGIN
    PRINT 'Création de l''index IX_DelegationRequest_RequesterId_Status...'
    CREATE NONCLUSTERED INDEX [IX_DelegationRequest_RequesterId_Status]
        ON [account].[DelegationRequest]([RequesterId] ASC, [Status] ASC);
    PRINT 'Index IX_DelegationRequest_RequesterId_Status créé.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DelegationRequest_Unique_Pending' 
               AND object_id = OBJECT_ID(N'[account].[DelegationRequest]'))
BEGIN
    PRINT 'Création de l''index unique IX_DelegationRequest_Unique_Pending...'
    CREATE UNIQUE NONCLUSTERED INDEX [IX_DelegationRequest_Unique_Pending]
        ON [account].[DelegationRequest]([RequesterId] ASC, [RecipientId] ASC, [AccountId] ASC)
        WHERE [Status] = 'pending';
    PRINT 'Index IX_DelegationRequest_Unique_Pending créé.'
END
GO

-- Ajouter les extended properties si elles n'existent pas
IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'DelegationRequestId')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'L''identifiant technique de la demande de délégation',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'DelegationRequestId'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'RequesterId')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'L''identifiant du contact demandeur',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'RequesterId'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'RecipientId')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'L''identifiant du contact destinataire',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'RecipientId'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'AccountId')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'L''identifiant du dossier concerné par la demande',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'AccountId'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'CreatedAt')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'La date de création de la demande',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'CreatedAt'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'Status')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'Le statut de la demande (pending, accepted, refused)',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'Status'
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.extended_properties 
    WHERE major_id = OBJECT_ID('[account].[DelegationRequest]')
    AND minor_id = (SELECT column_id FROM sys.columns 
                    WHERE object_id = OBJECT_ID('[account].[DelegationRequest]') 
                    AND name = 'RespondedAt')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sp_addextendedproperty @name = N'MS_Description',
        @value = N'La date de réponse à la demande (acceptation ou refus)',
        @level0type = N'SCHEMA',
        @level0name = N'account',
        @level1type = N'TABLE',
        @level1name = N'DelegationRequest',
        @level2type = N'COLUMN',
        @level2name = N'RespondedAt'
END
GO

PRINT 'Script de migration 006_Create_DelegationRequest_Table.sql terminé.'

