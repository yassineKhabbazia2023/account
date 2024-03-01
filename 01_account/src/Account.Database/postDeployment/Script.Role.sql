DECLARE @accountRothschild int, @contactRothschild int, @accountRenault int, @contactRenault int, @accountAsltom int, @contactAlstom1 int, @contactAlstom2 int, @accountSrp int, @contactSrp int;
DECLARE @accountImagotag int, @contactImagotag1 int, @contactImagotag2 int, @accountFleury int, @contactFleury int, @accountChaussLouis int, @contactChaussLouis int, @accountAbricotine int;
DECLARE @contactAbricotine int, @accountMogador int, @accountNexity int, @contactNexity int;

SET @accountRothschild = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999006639');
SET @contactRothschild = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'A88145B3-5D8E-46AC-93F7-EA3D52F51DCE');
SET @accountRenault = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999002380');
SET @contactRenault = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '440A2298-E4FB-48D7-BB75-C6648F2BAC09');
SET @accountAsltom = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999005090');
SET @contactAlstom1 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '4F1F3373-B600-4141-A657-001B687ED7AF');
SET @contactAlstom2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '7121FF33-9249-4A14-BD80-0039352AA671');
SET @accountSrp = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999012408');
SET @contactSrp = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'C9FCC19E-8DD1-4B1A-A426-1C0154A4008B');
SET @accountImagotag = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1000310871');
SET @contactImagotag1 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '86E74192-43FC-49BE-941E-0425C77077EC');
SET @contactImagotag2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'D6994F9A-017A-4144-AF47-F973AB517268');
SET @accountFleury = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1000613508');
SET @contactFleury = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '7E3BD4CA-97A8-42FF-BB16-F27E6D71F5BF');
SET @accountChaussLouis = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999032583');
SET @contactChaussLouis = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '00440B5C-FC87-4758-8886-604A89F9E4C7');
SET @accountAbricotine = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999117725');
SET @contactAbricotine = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '79FE3A8C-1046-428D-B9EA-D553328FAA60');
SET @accountMogador = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999154806');
SET @accountNexity = (SELECT AccountId FROM account.Account WHERE AccountNumber = '1999003568');
SET @contactNexity = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = '24B47B42-8A1E-45E1-A976-6AE844778A97');


INSERT INTO account.Role (AccountId, ContactId, IsFavorite, IsSignatory)
VALUES
(@accountRothschild, @contactRothschild, 1, 1),
(@accountRenault, @contactRenault, 1, 1),
(@accountAsltom, @contactAlstom1, 1, 1),
(@accountAsltom, @contactAlstom2, 0, 0),
(@accountSrp, @contactSrp, 0, 1),
(@accountImagotag, @contactImagotag1, 1, 0),
(@accountImagotag, @contactImagotag2, 0, 1),
(@accountFleury, @contactFleury, 1, 1),
(@accountChaussLouis, @contactChaussLouis, 1, 1),
(@accountAbricotine, @contactAbricotine, 1, 1),
(@accountAbricotine, @contactAlstom2, 0, 0),
(@accountMogador, @contactImagotag1, 0, 1),
(@accountNexity, @contactNexity, 1, 1);