CREATE TABLE [account].[Deployment]
(
	[DeploymentId]			INT IDENTITY(1, 1)	NOT NULL,
	[AccountId]				INT					NOT NULL,
	[DeploymentDate]        DATETIME2           NULL,
	[Status]                INT	                NOT NULL,
	CONSTRAINT [C_Deployment_PK] PRIMARY KEY NONCLUSTERED ([DeploymentId] ASC),
	CONSTRAINT [C_Account_Deployment_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IX_Deployment_DeploymentId]
    ON  [account].[Deployment]([DeploymentId] ASC);
GO
CREATE CLUSTERED INDEX [IXC_Deployment_AccountId]
    ON  [account].[Deployment]([AccountId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Deployment_Status]
    ON  [account].[Deployment]([Status] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Deployment',
    @level2type = N'COLUMN',
    @level2name = N'DeploymentId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Deployment',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date à laquelle le déploiement a eu lieu ',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Deployment',
    @level2type = N'COLUMN',
    @level2name = N'DeploymentDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le statut du déploiement',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Deployment',
    @level2type = N'COLUMN',
    @level2name = 'Status'