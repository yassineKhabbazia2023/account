CREATE TABLE [sch_acc].[THub]
(
	[HubId]          INT IDENTITY(1,1)	NOT NULL,
	[HubName]        VARCHAR(150)       NOT NULL,
	CONSTRAINT [C_THub_PK] PRIMARY KEY CLUSTERED ([HubId] ASC)
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'THub',
    @level2type = N'COLUMN',
    @level2name = N'HubId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nom du Hub',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'THub',
    @level2type = N'COLUMN',
    @level2name = N'HubName'