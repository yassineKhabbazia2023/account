IF NOT EXISTS (SELECT 1 FROM actor.Contact WHERE ContactGlobalUniqueId = 'A88145B3-5D8E-46AC-93F7-EA3D52F51DCE')
BEGIN
	INSERT INTO actor.Contact (ContactGlobalUniqueId, FirstName, LastName, ContactEmail, Type)
	VALUES
	('A88145B3-5D8E-46AC-93F7-EA3D52F51DCE', 'Patrick', 'BROUARD', 'pbrouard@rothschild.com', 'client'),
	('440A2298-E4FB-48D7-BB75-C6648F2BAC09', 'Jean-Paul', 'VELLUTINI', 'jvellutini@kpmg.fr', 'collaborator'),
	('7121FF33-9249-4A14-BD80-0039352AA671', 'Nadia', 'ALLIK', 'nallik@kpmg.fr', 'collaborator'),
	('4F1F3373-B600-4141-A657-001B687ED7AF', 'Henri', 'POUPART LAFARGE', 'hpoupartlafarge@asltom.fr', 'client'),
	('C9FCC19E-8DD1-4B1A-A426-1C0154A4008B', 'Stéphanie', 'ORTEGA', 'sortega@kpmg.fr', 'collaborator'),
	('86E74192-43FC-49BE-941E-0425C77077EC', 'Hervé', 'MICHELET', 'hmichelet@kpmg.fr', 'collaborator'),
	('D6994F9A-017A-4144-AF47-F973AB517268', 'Kévin', 'MICHELET', 'kevinmichelet39@gmail.com', 'client'),
	('7E3BD4CA-97A8-42FF-BB16-F27E6D71F5BF', 'Franck', 'NOEL', 'noel.franck@aliceadsl.fr', 'client'),
	('00440B5C-FC87-4758-8886-604A89F9E4C7', 'Bruno', 'LE BOZEC', 'chaussure.louis@orange.fr', 'client'),
	('79FE3A8C-1046-428D-B9EA-D553328FAA60', 'Yoann', 'DUCROS', 'yoann.ducros@gmail.com', 'client'),
	('24B47B42-8A1E-45E1-A976-6AE844778A97', 'Philippe', 'MATHIS', 'pmathis@kpmg.fr', 'collaborator');
END