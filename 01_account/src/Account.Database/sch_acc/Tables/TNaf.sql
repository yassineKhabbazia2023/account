CREATE TABLE [sch_acc].[TNaf]
(
	[NafId]			INT IDENTITY(1, 1)	NOT NULL,
	[NafCode]		VARCHAR(10)			NOT NULL,
	[AccountId]     INT                 NOT NULL,
	CONSTRAINT [C_TNaf_PK] PRIMARY KEY CLUSTERED ([NafId] ASC),
	CONSTRAINT [C_TAccount_TNaf_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TNaf_AccountId]
    ON   [sch_acc].[TNaf]([AccountId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = N'NafId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code Naf de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = N'NafCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le niveau de détail du libellé associé au code NAF',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = 'NafLabel'