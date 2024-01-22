CREATE TABLE [sch_acc].[TAddress]
(
	[AddressId]     INT IDENTITY(1, 1) NOT NULL,
    [AccountId]     INT                NOT NULL,
	[Street]        NVARCHAR(MAX)      NOT NULL,
    [City]          VARCHAR(50)        NOT NULL,
    [State]         VARCHAR(50)        NULL,
    [ZipCode]       VARCHAR(20)        NOT NULL,
    [Country]       VARCHAR(25)        NOT NULL,
    [AddressType]   VARCHAR(25)        NULL,
    CONSTRAINT [C_TAddress_PK] PRIMARY KEY CLUSTERED ([AddressId] ASC),
    CONSTRAINT [C_TAccount_TAddress_FK] FOREIGN KEY ([AccountId]) REFERENCES [sch_acc].[TAccount] ([AccountId]),
)

GO
CREATE NONCLUSTERED INDEX [IDX_TAddress_AccountId]
    ON [sch_acc].[TAddress]([AccountId] ASC);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le type d''adresse',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'AddressType'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le pays',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'Country'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le code postal',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'ZipCode'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Le département',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'State'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La ville',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'City'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La rue',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'Street'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'sch_acc',
    @level1type = N'TABLE',
    @level1name = N'TAddress',
    @level2type = N'COLUMN',
    @level2name = N'AddressId'