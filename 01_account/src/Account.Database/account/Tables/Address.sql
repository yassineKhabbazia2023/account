CREATE TABLE [account].[Address] (
    [AddressId]    INT            IDENTITY (1, 1) NOT NULL,
    [AccountId]    INT            NOT NULL,
    [AddressLine1] NVARCHAR (255) NULL,
    [AddressLine2] NVARCHAR (255) NULL,
    [AddressLine3] NVARCHAR (255) NULL,
    [ZipCode]      VARCHAR (20)   NOT NULL,
    [City]         VARCHAR (50)   NOT NULL,
    [State]        VARCHAR (50)   NULL,
    [Country]      VARCHAR (50)   NOT NULL,
    [AddressType]  VARCHAR (25)   NOT NULL,
    CONSTRAINT [C_Address_PK] PRIMARY KEY CLUSTERED ([AddressId] ASC),
    CONSTRAINT [C_Account_Address_AccountId_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
);



GO
CREATE NONCLUSTERED INDEX [IDX_Address_AccountId]
    ON  [account].[Address]([AccountId] ASC)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type d''adresse',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'AddressType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le pays',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'Country'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code postal',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'ZipCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le département',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'State'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La ville',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'City'
GO

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'AddressId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Address',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'