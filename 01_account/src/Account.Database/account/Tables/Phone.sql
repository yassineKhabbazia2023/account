CREATE TABLE [account].[Phone]
(
	[PhoneId]			INT IDENTITY(1, 1)	NOT NULL,
    [AccountId]         INT                 NOT NULL,       
	[PhoneNumber]       VARCHAR(20)			NOT NULL,
	[Type]			    VARCHAR(25)         NULL,
	CONSTRAINT [C_Phone_PK] PRIMARY KEY CLUSTERED ([PhoneId] ASC),
    CONSTRAINT [C_Account_Phone_AccountId_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
)

GO
CREATE NONCLUSTERED INDEX [IDX_Phone_AccountId]
    ON  [account].[Phone]([AccountId] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Phone',
    @level2type = N'COLUMN',
    @level2name = 'PhoneId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le numéro de téléphone',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Phone',
    @level2type = N'COLUMN',
    @level2name = 'PhoneNumber'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type du numéro de téléphone',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Phone',
    @level2type = N'COLUMN',
    @level2name = 'Type'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant techique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Phone',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'