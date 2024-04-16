INSERT INTO [account].[Address] ([AccountId], [AddressLine1], [AddressLine2], [AddressLine3], [ZipCode], [City], [State], [Country], [AddressType])
VALUES 
(601, '123 Rue de la Mer', NULL, NULL, '76600', 'Le Havre', NULL, 'France', 'billing'),
(602, '456 Avenue des Marins', NULL, NULL, '76610', 'Le Havre', NULL, 'France', 'billing'),
(603, '789 Boulevard de l''Océan', NULL, NULL, '76620', 'Le Havre', NULL, 'France', 'billing'),
(601, '321 Rue du Port', NULL, NULL, '76630', 'Le Havre', NULL, 'France', 'delivery'),
(602, '654 Quai de la République', NULL, NULL, '76640', 'Le Havre', NULL, 'France', 'delivery'),
(603, '987 Rue Gustave Flaubert', NULL, NULL, '76650', 'Le Havre', NULL, 'France', 'delivery');
