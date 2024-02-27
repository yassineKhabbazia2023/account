DECLARE @accountRothschild int, @accountRenault int, @accountAsltom int, @accountSrp int, @accountImagotag int, @accountFleury int;
DECLARE @accountChaussLouis int, @accountAbricotine int, @accountMogador int, @accountNexity int;

SET @accountRothschild = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999006639');
SET @accountRenault = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999002380');
SET @accountAsltom = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999005090');
SET @accountSrp = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999012408');
SET @accountImagotag = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1000310871');
SET @accountFleury = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1000613508');
SET @accountChaussLouis = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999032583');
SET @accountAbricotine = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999117725');
SET @accountMogador = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999154806');
SET @accountNexity = (SELECT AccountId FROM account.TAccount WHERE AccountNumber = '1999003568');

INSERT INTO account.TAddress (AccountId, AddressLine1, AddressLine2, AddressLine3, ZipCode, City, State, Country, AddressType)
VALUES 
(@accountRothschild, '23 bis  avenue de Messine', NULL, NULL, '75008', 'Paris', NULL, 'France', 'delivery'),
(@accountRothschild, '23 bis  avenue de Messine', NULL, NULL, '75008', 'Paris', NULL, 'France', 'billing'),
(@accountRenault, '122-122 bis avenue du Général Leclerc', NULL, NULL, '92100', 'Boulogne-Billancourt', NULL, 'France', 'delivery'),
(@accountRenault, '122-122 bis avenue du Général Leclerc', NULL, NULL, '92100', 'Boulogne-Billancourt', NULL, 'France', 'billing'),
(@accountAsltom, '48 rue Albert Dhalenne', NULL, NULL, '93400', 'Saint-Ouen-sur-Seine', NULL, 'France', 'delivery'),
(@accountAsltom, '48 rue Albert Dhalenne', NULL, NULL, '93400', 'Saint-Ouen-sur-Seine', NULL, 'France', 'billing'),
(@accountSrp, '1 rue des Blés', 'ZAC de la Montjoie', NULL, '93212', 'Plaine-Saint-Denis CEDEX', NULL, 'France', 'delivery'),
(@accountSrp, '1 rue des Blés', 'ZAC de la Montjoie', NULL, '93212', 'Plaine-Saint-Denis CEDEX', NULL, 'France', 'billing'),
(@accountImagotag, '55 place Nelson Mandela', NULL, NULL, '92000', 'Nanterre', NULL, 'France', 'delivery'),
(@accountImagotag, '55 place Nelson Mandela', NULL, NULL, '92000', 'Nanterre', NULL, 'France', 'billing'),
(@accountFleury, 'ROUTE DE LA GARE', NULL, NULL, '85700', 'Pouzauges', NULL, 'France', 'delivery'),
(@accountFleury, 'ROUTE DE LA GARE', NULL, NULL, '85700', 'Pouzauges', NULL, 'France', 'billing'),
(@accountChaussLouis, 'Avenue Ambroise Croizat', NULL, NULL, '38920', 'Crolles', NULL, 'France', 'delivery'),
(@accountChaussLouis, 'Avenue Ambroise Croizat', NULL, NULL, '38920', 'Crolles', NULL, 'France', 'billing'),
(@accountAbricotine, 'Route de Fouillouse', NULL, NULL, '26300', 'Chateauneuf-sur-Isère', NULL, 'France', 'delivery'),
(@accountAbricotine, 'Route de Fouillouse', NULL, NULL, '26300', 'Chateauneuf-sur-Isère', NULL, 'France', 'billing'),
(@accountMogador, 'Rue DE CERDAGNE', NULL, NULL, '66140', 'Canet-en-Roussillon', NULL, 'France', 'delivery'),
(@accountMogador, 'Rue DE CERDAGNE', NULL, NULL, '66140', 'Canet-en-Roussillon', NULL, 'France', 'billing'),
(@accountNexity, '19, rue de Vienne - TSA 50029', NULL, NULL, '75801', 'PARIS CEDEX 08', NULL, 'France', 'delivery'),
(@accountNexity, '19, rue de Vienne - TSA 50029', NULL, NULL, '75801', 'PARIS CEDEX 08', NULL, 'France', 'billing');