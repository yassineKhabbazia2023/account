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

GO
CREATE NONCLUSTERED INDEX [IX_DelegationRequest_RecipientId_Status]
    ON [account].[DelegationRequest]([RecipientId] ASC, [Status] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_DelegationRequest_RequesterId_Status]
    ON [account].[DelegationRequest]([RequesterId] ASC, [Status] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DelegationRequest_Unique_Pending]
    ON [account].[DelegationRequest]([RequesterId] ASC, [RecipientId] ASC, [AccountId] ASC)
    WHERE [Status] = 'pending';

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de la demande de délégation',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'DelegationRequestId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du contact demandeur',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'RequesterId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du contact destinataire',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'RecipientId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant du dossier concerné par la demande',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de création de la demande',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'CreatedAt'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le statut de la demande (pending, accepted, refused)',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'Status'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date de réponse à la demande (acceptation ou refus)',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'DelegationRequest',
    @level2type = N'COLUMN',
    @level2name = N'RespondedAt'
GO

