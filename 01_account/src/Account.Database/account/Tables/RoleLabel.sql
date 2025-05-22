CREATE TABLE [account].[RoleLabel]
(
	AccountId INT NOT NULL,
	ContactId INT NOT NULL, 
	LabelId INT NOT NULL, 
	[CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	PRIMARY KEY (AccountId,ContactId,LabelId),
	Foreign Key (AccountId) References account.Account(AccountId),
	Foreign key (ContactId) References actor.Contact(ContactId),
	Foreign key (CreatedBy) References actor.Contact(ContactId),
	Foreign key (LabelId) References account.[Label](LabelId)
)
