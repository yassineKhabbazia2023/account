BEGIN TRANSACTION;

DROP INDEX [IX_Account_AccountNumber] ON [account].[Account];
DROP INDEX [IX_Contact_Email_LastName_FirstName_IsActive] ON [actor].[Contact];

-- 1️⃣ Mettre à jour les données en tronquant à {n} caractères
-- ACCOUNT
UPDATE [account].[Account]
SET AccountNumber = LEFT(AccountNumber, 20);

UPDATE [account].[Account]
SET Siret = LEFT(Siret, 14);

--CONTACT
UPDATE [actor].[Contact]
SET FirstName = LEFT(FirstName, 100);

UPDATE [actor].[Contact]
SET LastName = LEFT(LastName, 100);

UPDATE [actor].[Contact]
SET Email = LEFT(Email, 100);

-- 2️⃣ Modifier la colonne pour réduire la taille à VARCHAR(n)
--ACCOUNT
ALTER TABLE [account].[Account]
ALTER COLUMN AccountNumber VARCHAR(20);

ALTER TABLE [account].[Account]
ALTER COLUMN Siret VARCHAR(14);

--CONTACT
ALTER TABLE [actor].[Contact]
ALTER COLUMN FirstName VARCHAR(100);

ALTER TABLE [actor].[Contact]
ALTER COLUMN LastName VARCHAR(100);

ALTER TABLE [actor].[Contact]
ALTER COLUMN Email VARCHAR(100);

CREATE NONCLUSTERED INDEX [IX_Account_AccountNumber]
    ON  [account].[Account]([AccountNumber] ASC);

CREATE NONCLUSTERED INDEX [IX_Contact_Email_LastName_FirstName_IsActive]
    ON  [actor].[Contact]([Email], [LastName], [FirstName], [IsActive]);

COMMIT TRANSACTION;