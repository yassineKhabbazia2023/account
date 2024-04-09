IF NOT EXISTS (SELECT * FROM [actor].[Contact] WHERE ContactId = 56)
BEGIN
INSERT into actor.Contact(ContactId, ContactGlobalUniqueId, FirstName, LastName, Email, Type, Status, CreationDate, PersonaName, Office)
VALUES
(56,NEWID(),'Alice','Durand','alice.durand@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(57,NEWID(),'Pierre','Dubois','pierre.dubois@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(58,NEWID(),'Élodie','Martin','elodie.martin@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(59,NEWID(),'Antoine','Leroy','antoine.leroy@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(60,NEWID(),'Camille','Moreau','camille.moreau@example.com','Customer','Declared',GETDATE(),'RH',null),
(61,NEWID(),'Charlotte','Lefebvre','charlotte.lefebvre@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(62,NEWID(),'Lucas','Girard','lucas.girard@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(63,NEWID(),'Emma','Roux','emma.roux@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(64,NEWID(),'Louis','Bonnet','louis.bonnet@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(65,NEWID(),'Manon','François','manon.francois@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(66,NEWID(),'Théo','Martinez','theo.martinez@example.com','Customer','Declared',GETDATE(),'RH',null),
(67,NEWID(),'Jade','Fournier','jade.fournier@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(68,NEWID(),'Hugo','Lopez','hugo.lopez@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(69,NEWID(),'Léa','Sanchez','lea.sanchez@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(70,NEWID(),'Maëlys','Meunier','maelys.meunier@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(71,NEWID(),'Paul','Robin','paul.robin@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(72,NEWID(),'Louise','Petit','louise.petit@example.com','Customer','Declared',GETDATE(),'RH',null),
(73,NEWID(),'Hugo','Dubois','hugo.dubois@example.com','Customer','Declared',GETDATE(),'Comptable', null)
END