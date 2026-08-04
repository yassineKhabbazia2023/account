-- Table definition for Invoice
CREATE TABLE [account].[Invoice]
(
	[InvoiceId]		    INT		NOT NULL IDENTITY(1,1),
	[InvoiceNumber]		VARCHAR(50)	NOT NULL,
	[DocumentPath]		NVARCHAR(4000)	NOT NULL,
	[Type]				VARCHAR(50)	NOT NULL,
	[Category]			VARCHAR(50)	NOT NULL,
	[InvoiceDate]	    DATETIME2		NOT NULL,
	[DepositDate]	    DATETIME2		NOT NULL DEFAULT GETDATE(),
	[AccountId]		    INT		NOT NULL,
	CONSTRAINT [C_Invoice_PK] PRIMARY KEY CLUSTERED ([InvoiceId] ASC),
	CONSTRAINT [C_Account_Invoice_AccountId_FK] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
	CONSTRAINT [UQ_Invoice_InvoiceNumber] UNIQUE ([InvoiceNumber])
);

GO

CREATE NONCLUSTERED INDEX [IX_Invoice_AccountId]
	ON [account].[Invoice] ([AccountId] ASC);

GO

-- Extended Properties
EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'L''identifiant technique',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'InvoiceId'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'Le numéro de facture (identifiant externe unique)',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'InvoiceNumber'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'Le chemin du document (nom affiché = dernier segment)',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'DocumentPath'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'Le type de facture (ex: Facture RYDGE)',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'Type'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'La catégorie de facture',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'Category'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'La date de facturation',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'InvoiceDate'
GO

EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'La date de dépôt',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'DepositDate'
GO


EXEC sp_addextendedproperty @name = N'MS_Description',
	@value = N'L''identifiant technique de l''entité',
	@level0type = N'SCHEMA',
	@level0name = N'account',
	@level1type = N'TABLE',
	@level1name = N'Invoice',
	@level2type = N'COLUMN',
	@level2name = N'AccountId'
GO
