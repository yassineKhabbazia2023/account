DECLARE @accountRothschild int, @contactRothschild int, @contactRenault int, @accountAsltom int, @contactAlstom1 int, @contactAlstom2 int;
DECLARE @accountImagotag int, @contactImagotag1 int, @contactImagotag2 int, @accountFleury int, @contactFleury int;
DECLARE @accountNexity int, @contactNexity1 int, @contactNexity2 int;

SET @accountRothschild = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999006639');
SET @contactRothschild = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'A88145B3-5D8E-46AC-93F7-EA3D52F51DCE');
SET @contactRenault = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '440A2298-E4FB-48D7-BB75-C6648F2BAC09');
SET @accountAsltom = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999005090');
SET @contactAlstom1 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '4F1F3373-B600-4141-A657-001B687ED7AF');
SET @contactAlstom2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '7121FF33-9249-4A14-BD80-0039352AA671');
SET @accountImagotag = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1000310871');
SET @contactImagotag1 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '86E74192-43FC-49BE-941E-0425C77077EC');
SET @contactImagotag2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'D6994F9A-017A-4144-AF47-F973AB517268');
SET @accountFleury = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1000613508');
SET @contactFleury = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '7E3BD4CA-97A8-42FF-BB16-F27E6D71F5BF');
SET @accountNexity = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999003568');
SET @contactNexity1 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '24B47B42-8A1E-45E1-A976-6AE844778A97');
SET @contactNexity2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'D6994F9A-017A-4144-AF47-F973AB517268');

INSERT INTO account.Delegation (AccountId, DelegatorId, DelegateeId, StartDate, EndDate, Status, Note, CreationDate)
VALUES
(@accountRothschild, @contactRothschild, @contactRenault, CAST ('2019-02-11' AS datetime2), CAST ('2019-03-01' AS datetime2), 'disabled', 'Délégation spéciale', SYSDATETIME()),
(@accountAsltom, @contactAlstom1, @contactAlstom2, CAST ('1999-08-09' AS datetime2), CAST ('2000-02-21' AS datetime2), 'disabled', NULL, SYSDATETIME()),
(@accountAsltom, @contactAlstom1, @contactRenault, CAST ('2024-01-01' AS datetime2), NULL, 'enabled', 'Délégation long terme', SYSDATETIME()),
(@accountImagotag, @contactImagotag2, @contactImagotag1, CAST ('2022-07-01' AS datetime2), CAST ('2022-09-01' AS datetime2), 'disabled', 'Remplacement', SYSDATETIME()),
(@accountImagotag, @contactImagotag2, @contactImagotag1, CAST ('2024-05-17' AS datetime2), CAST ('2025-01-31' AS datetime2), 'pending', 'Demande de délégation', SYSDATETIME()),
(@accountFleury, @contactFleury, @contactAlstom2, CAST ('2023-11-29' AS datetime2), NULL, 'pending', 'Je voudrais donner les droits à partir du 29 nov 2023 pour une durée indéterminée', SYSDATETIME()),
(@accountNexity, @contactNexity1, @contactNexity2, CAST ('2024-02-07' AS datetime2), CAST ('2024-04-01' AS datetime2), 'enabled', 'En cours', SYSDATETIME());