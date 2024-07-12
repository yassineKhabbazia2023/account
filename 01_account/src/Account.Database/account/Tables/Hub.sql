CREATE TABLE [account].[Hub]
(
	[HubId]          INT IDENTITY(1,1)	NOT NULL,
	[HubName]        VARCHAR(150)       NOT NULL,
	CONSTRAINT [C_Hub_PK] PRIMARY KEY CLUSTERED ([HubId] ASC)
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Hub',
    @level2type = N'COLUMN',
    @level2name = N'HubId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le nom du Hub',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Hub',
    @level2type = N'COLUMN',
    @level2name = N'HubName'