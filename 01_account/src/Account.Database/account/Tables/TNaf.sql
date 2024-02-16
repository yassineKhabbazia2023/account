CREATE TABLE [account].[TNaf] (
    [NafId]    INT           IDENTITY (1, 1) NOT NULL,
    [NafCode]  VARCHAR (10)  NOT NULL,
    [NafLabel] VARCHAR (100) NULL,
    CONSTRAINT [C_TNaf_PK] PRIMARY KEY CLUSTERED ([NafId] ASC)
);



GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = N'NafId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code Naf de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'TNaf',
    @level2type = N'COLUMN',
    @level2name = N'NafCode'
GO