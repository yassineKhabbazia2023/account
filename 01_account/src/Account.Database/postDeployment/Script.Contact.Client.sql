IF NOT EXISTS (SELECT * FROM [actor].[Contact] WHERE ContactId = 56)
BEGIN
INSERT into actor.Contact(ContactId, ContactGlobalUniqueId, FirstName, LastName, Email, Type, Status, CreationDate, PersonaName, Office)
VALUES
(56,'3A97640F-B16E-4607-94FC-BE9C9D5E45C1','Alice','Durand','alice.durand@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(57,'A9A12F96-D8AE-4392-9B81-DEF909BBAF15','Pierre','Dubois','pierre.dubois@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(58,'57123B28-546E-4FD9-947D-66FBEBBEBEE8','Élodie','Martin','elodie.martin@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(59,'3FE2C258-FA37-4236-ADCA-64E8BFB82F67','Antoine','Leroy','antoine.leroy@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(60,'F7086A19-E726-42DB-9FEC-E2C1BB7484F6','Camille','Moreau','camille.moreau@example.com','Customer','Declared',GETDATE(),'RH',null),
(61,'44E5F5FE-4D44-4C69-9F54-9F40DA688720','Charlotte','Lefebvre','charlotte.lefebvre@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(62,'D4495635-491A-43BA-B6BA-2D367CA52347','Lucas','Girard','lucas.girard@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(63,'F6B21E9F-9028-4DBC-AC4C-EFEA685FA373','Emma','Roux','emma.roux@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(64,'A4281A12-E79D-4A97-A485-5DDF66113585','Louis','Bonnet','louis.bonnet@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(65,'FADCB6D6-B1ED-4E18-AE9E-0A7C520A8ACA','Manon','François','manon.francois@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(66,'59E7059E-10F7-4BFD-BDD0-FDEF14627D4C','Théo','Martinez','theo.martinez@example.com','Customer','Declared',GETDATE(),'RH',null),
(67,'AB3EA240-77ED-42A6-BF42-AF93BEE6E7A2','Jade','Fournier','jade.fournier@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(68,'C65AB52A-89A4-4EB7-93E5-5E3BB2D54676','Hugo','Lopez','hugo.lopez@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(69,'20260796-0B68-4D2B-8303-F835AC5E9948','Léa','Sanchez','lea.sanchez@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(70,'1B9DBD68-4DE9-4B4A-83C5-57E96CE6ABAA','Maëlys','Meunier','maelys.meunier@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(71,'A4E462E3-4CFD-4471-8623-27C38D33A437','Paul','Robin','paul.robin@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(72,'83A4FA34-D90C-4606-B7A4-6A88AE3B2412','Louise','Petit','louise.petit@example.com','Customer','Declared',GETDATE(),'RH',null),
(73,'F0EB61A1-A4B4-44EF-B783-E76025014358','Hugo','Dubois','hugo.dubois@example.com','Customer','Declared',GETDATE(),'Comptable',null)
,(188,'F0EB61A1-A4B4-44EF-B783-E76025014300','Olympe','GREC','userdemo2@test.fr','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null)
,(189,'F0EB61A1-A4B4-44EF-B783-E76025014301','Sully','MAN','slescot+CP1@kpmg.onmicrosoft.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null)
END