IF NOT EXISTS (SELECT 1 FROM [account].[Label] WHERE [Code] = 'CLP')
BEGIN
	INSERT INTO [account].[Label] ([Code], [CustomerLabel], [CollaboratorLabel], [Description], [Business], [IsVisible])
	VALUES ('CLP', 'Responsable', 'Maitre dossier', 'Maitre dossier', 'Transverse', 0),
	('AM', 'Chargé de mission', 'Resp compte', 'Resp compte', 'Transverse', 0),
	('HM', 'Chef de mission', 'Chef de mission', 'Chef de mission', 'ESC', 1),
	('AC', 'Expert comptable', 'Expert comptable', 'Expert comptable', 'ESC', 1),
	('PC', 'Collaborateur principal', 'Collaborateur principal', 'Collaborateur principal', 'ESC', 1),
	('SC', 'Collaborateur secondaire', 'Collaborateur secondaire', 'Collaborateur secondaire', 'ESC', 1),
	('AP', 'Partenaire conseil', 'Partenaire conseil', 'Partenaire conseil', 'Conseil', 1)
END

-- Migration des libellés pour les existants
UPDATE [account].[Label] SET [CollaboratorLabel] = 'Maitre dossier', [Description] = 'Maitre dossier' WHERE [Code] = 'CLP' AND [CollaboratorLabel] <> 'Maitre dossier';
UPDATE [account].[Label] SET [CollaboratorLabel] = 'Resp compte', [Description] = 'Resp compte' WHERE [Code] = 'AM' AND [CollaboratorLabel] <> 'Resp compte';