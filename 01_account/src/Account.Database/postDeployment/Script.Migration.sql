if not exists (select * from sysobjects where name='tmp_delegation_detail' and xtype='U')
CREATE TABLE [dbo].[tmp_delegation_detail](
	[DelegatorId] [int] NULL,
	[DelegateeId] [int] NULL,
	[AccountId] [int] NULL,
) ON [PRIMARY]
GO

-- Exécuter le script de migration pour la table DelegationRequest
:r ..\Scripts\006_Create_DelegationRequest_Table.sql

