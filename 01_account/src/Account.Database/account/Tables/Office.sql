CREATE TABLE [account].[Office]
(
	[OfficeId]      INT IDENTITY (1, 1) NOT NULL,
	[Name]          VARCHAR(255),
    [PhoneNumber]   VARCHAR(20),
    [AddressId]     INT NOT NULL,
    CONSTRAINT [C_Office_PK] PRIMARY KEY ([OfficeId]),
    CONSTRAINT [C_Account_Office_AddressId_FK] FOREIGN KEY ([AddressId]) REFERENCES [account].[Address] ([AddressId])
)
Go

CREATE NONCLUSTERED INDEX [IX_Address_AddressId]
    ON  [account].[Office]([AddressId] ASC)

GO
