-- Table definition for Invoice
CREATE TABLE [account].[Invoice]
(
	[InvoiceId]		    INT		NOT NULL IDENTITY(1,1),
	[InvoiceNumber]		VARCHAR(255)	NOT NULL,
	[Name]			        NVARCHAR(255)	NOT NULL,
	[InvoiceDate]	        DATETIME2	NOT NULL,
	[DepositDate]	        DATETIME2	NOT NULL,
	[AccountId]		        INT		NOT NULL,
	CONSTRAINT [C_Invoice_PK] PRIMARY KEY CLUSTERED ([InvoiceId] ASC),
	CONSTRAINT [C_Account_Invoice_AccountId_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
	CONSTRAINT [UQ_Invoice_InvoiceNumber] UNIQUE ([InvoiceNumber])
);

GO

CREATE NONCLUSTERED INDEX [IX_Invoice_AccountId]
	ON [account].[Invoice] ([AccountId] ASC);

GO

-- Extended Properties moved to Scripts/Extended_Properties_Invoice.sql to avoid SSDT compilation errors
