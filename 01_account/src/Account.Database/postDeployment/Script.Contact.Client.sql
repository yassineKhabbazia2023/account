IF NOT EXISTS (SELECT * FROM [actor].[Contact] WHERE ContactId = 56)
BEGIN
INSERT into actor.Contact(ContactId, ContactGlobalUniqueId, FirstName, LastName, Email, Type, Status, CreationDate, PersonaName, Office)
VALUES
(56,'6BBCE731-8504-446A-AFB1-AA18FF61698F','Alice','Durand','alice.durand@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(57,'59DD0126-4627-4857-9CCE-F24C365CF73A','Pierre','Dubois','pierre.dubois@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(58,'A197E562-DF3E-43F9-9CD3-F9CBFFEE1D35','Élodie','Martin','elodie.martin@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(59,'B000FAB7-3E7A-4C9F-BD3D-7B79A0904866','Antoine','Leroy','antoine.leroy@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(60,'19B41E27-B98D-4675-843E-5A300CDE7A39','Camille','Moreau','camille.moreau@example.com','Customer','Declared',GETDATE(),'RH',null),
(61,'F42246B3-0520-49EE-A67A-F3B839DD4678','Charlotte','Lefebvre','charlotte.lefebvre@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(62,'F19CA94A-5843-4CBD-AD23-5CFC084C6574','Lucas','Girard','lucas.girard@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(63,'0F13E8C6-5628-4677-9715-4CFAB238A25E','Emma','Roux','emma.roux@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(64,'F5040FD3-B683-4FFA-8B47-0B2C8D0F3A86','Louis','Bonnet','louis.bonnet@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(65,'2F3061AF-9667-4BA0-8167-6A6354CABFC6','Manon','François','manon.francois@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(66,'E1360ABE-2065-4E67-864B-81667D33E8DF','Théo','Martinez','theo.martinez@example.com','Customer','Declared',GETDATE(),'RH',null),
(67,'B98B05E3-427B-4CB8-9BA4-47C5A11D909B','Jade','Fournier','jade.fournier@example.com','Customer','Declared',GETDATE(),'Comptable',null),
(68,'7F015F4A-A303-430F-A807-C1521943FF5D','Hugo','Lopez','hugo.lopez@example.com','Customer','Declared',GETDATE(),'ADMIN/Dirigeant',null),
(69,'CE7557C6-0091-4F55-9472-02F0A53E43DD','Léa','Sanchez','lea.sanchez@example.com','Customer','Declared',GETDATE(),'ADMIN/Associé',null),
(70,'4B73A21D-AB3A-4E80-A854-A29318FB7B71','Maëlys','Meunier','maelys.meunier@example.com','Customer','Declared',GETDATE(),'Assistante',null),
(71,'F6BB3321-5009-49AE-A84C-3940A5091BEC','Paul','Robin','paul.robin@example.com','Customer','Declared',GETDATE(),'Responsable financier',null),
(72,'3F18FC2C-5E8A-4A4B-80E8-F1B410F27D19','Louise','Petit','louise.petit@example.com','Customer','Declared',GETDATE(),'RH',null),
(73,'8F272F53-6F44-407C-B498-438F5747AAA3','Hugo','Dubois','hugo.dubois@example.com','Customer','Declared',GETDATE(),'Comptable',null)
END