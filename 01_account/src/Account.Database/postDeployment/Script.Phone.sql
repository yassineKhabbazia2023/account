DECLARE @accountRothschild int, @accountRenault int, @accountAsltom int, @accountImagotag int, @accountFleury int, @accountChaussLouis int, @accountAbricotine int;

SET @accountRothschild = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999006639');
SET @accountRenault = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999002380');
SET @accountAsltom = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999005090');
SET @accountImagotag = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1000310871');
SET @accountFleury = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1000613508');
SET @accountChaussLouis = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999032583');
SET @accountAbricotine = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999117725');

INSERT INTO account.TPhone (AccountId, PhoneNumber, Type)
VALUES
(@accountRothschild, '+41 44 384 70 75', 'billing'),
(@accountRenault, '01 76 84 04 04', 'billing'),
(@accountAsltom, '01 57 06 90 00', 'delivery'),
(@accountAsltom, '01 57 06 90 00', 'billing'),
(@accountImagotag, '01 34 34 61 61', 'delivery'),
(@accountImagotag, '01 34 34 61 61', 'billing'),
(@accountFleury, '0251663232', 'billing'),
(@accountChaussLouis, '04 38 72 92 87', 'delivery'),
(@accountChaussLouis, '04 38 72 92 87', 'billing'),
(@accountAbricotine, '06 32 52 17 30', 'delivery'),
(@accountAbricotine, '06 32 52 17 30', 'billing');