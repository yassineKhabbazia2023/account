CREATE TABLE [sch_acc].[TPhone]
(
	[PhoneId]			INT IDENTITY(1, 1)	NOT NULL,
    [AccountId]         INT                 NOT NULL,       
	[PhoneNumber]       VARCHAR(15)			NOT NULL,
	[PhoneType]			VARCHAR(25)         NULL,
	CONSTRAINT [C_TPhone_PK] PRIMARY KEY CLUSTERED ([PhoneId] ASC),
    CONSTRAINT [C_TAccount_TPhone_AccountId_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_TPhone_AccountId]
    ON  [sch_acc].[TPhone]([AccountId] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TPhone',
    @level2type = N'COLUMN',
    @level2name = 'PhoneId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le numéro de téléphone',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TPhone',
    @level2type = N'COLUMN',
    @level2name = 'PhoneNumber'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type du numéro de téléphone',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TPhone',
    @level2type = N'COLUMN',
    @level2name = 'PhoneType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant techique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TPhone',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'