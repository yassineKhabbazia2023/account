IF NOT EXISTS (SELECT * FROM [actor].[Contact] WHERE ContactId = 56)
BEGIN
INSERT into actor.Contact(ContactId, ContactGlobalUniqueId, FirstName, LastName, Email, Type, Status, CreationDate, PersonaName, Office)
VALUES
(56,NEWID(),'Alice','Durand','alice.durand@example.com','customer','declared',GETDATE(),'ADMIN/Dirigeant',null),
(57,NEWID(),'Pierre','Dubois','pierre.dubois@example.com','customer','declared',GETDATE(),'ADMIN/Associé',null),
(58,NEWID(),'Élodie','Martin','elodie.martin@example.com','customer','declared',GETDATE(),'Assistante',null),
(59,NEWID(),'Antoine','Leroy','antoine.leroy@example.com','customer','declared',GETDATE(),'Responsable financier',null),
(60,NEWID(),'Camille','Moreau','camille.moreau@example.com','customer','declared',GETDATE(),'RH',null),
(61,NEWID(),'Charlotte','Lefebvre','charlotte.lefebvre@example.com','customer','declared',GETDATE(),'Comptable',null),
(62,NEWID(),'Lucas','Girard','lucas.girard@example.com','customer','declared',GETDATE(),'ADMIN/Dirigeant',null),
(63,NEWID(),'Emma','Roux','emma.roux@example.com','customer','declared',GETDATE(),'ADMIN/Associé',null),
(64,NEWID(),'Louis','Bonnet','louis.bonnet@example.com','customer','declared',GETDATE(),'Assistante',null),
(65,NEWID(),'Manon','François','manon.francois@example.com','customer','declared',GETDATE(),'Responsable financier',null),
(66,NEWID(),'Théo','Martinez','theo.martinez@example.com','customer','declared',GETDATE(),'RH',null),
(67,NEWID(),'Jade','Fournier','jade.fournier@example.com','customer','declared',GETDATE(),'Comptable',null),
(68,NEWID(),'Hugo','Lopez','hugo.lopez@example.com','customer','declared',GETDATE(),'ADMIN/Dirigeant',null),
(69,NEWID(),'Léa','Sanchez','lea.sanchez@example.com','customer','declared',GETDATE(),'ADMIN/Associé',null),
(70,NEWID(),'Maëlys','Meunier','maelys.meunier@example.com','customer','declared',GETDATE(),'Assistante',null),
(71,NEWID(),'Paul','Robin','paul.robin@example.com','customer','declared',GETDATE(),'Responsable financier',null),
(72,NEWID(),'Louise','Petit','louise.petit@example.com','customer','declared',GETDATE(),'RH',null),
(73,NEWID(),'Hugo','Dubois','hugo.dubois@example.com','customer','declared',GETDATE(),'Comptable',null)
END