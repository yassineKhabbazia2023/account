CREATE TABLE [sch_acc].[TDeploymentPlanning]
(
	[DeploymentId]			INT IDENTITY(1, 1)	NOT NULL,
	[AccountId]				INT					NOT NULL,
	[DeploymentDate]        DATETIME2           NOT NULL,
	[Status]                INT	                NOT NULL,
	CONSTRAINT [C_TDeploymentPlanning_PK] PRIMARY KEY CLUSTERED ([DeploymentId] ASC),
	CONSTRAINT [C_TAccount_TDeployment_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TDeploymentPlanning_AccountId]
    ON  [sch_acc].[TDeploymentPlanning]([AccountId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDeploymentPlanning',
    @level2type = N'COLUMN',
    @level2name = N'DeploymentId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDeploymentPlanning',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La date à laquelle le déploiement a eu lieu ',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDeploymentPlanning',
    @level2type = N'COLUMN',
    @level2name = N'DeploymentDate'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le statut du déploiement',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TDeploymentPlanning',
    @level2type = N'COLUMN',
    @level2name = 'Status'