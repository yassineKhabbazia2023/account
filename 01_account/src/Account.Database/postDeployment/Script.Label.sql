IF NOT EXISTS (SELECT 1 FROM [account].[Label] WHERE [Code] = 'CLP')
BEGIN
	INSERT INTO [account].[Label] ([Code], [CustomerLabel], [CollaboratorLabel], [Description], [Business], [IsVisible])
	VALUES ('CLP', 'Responsable de Compte', 'Responsable de Compte', 'Responsable de Compte', 'Transverse', 0),
	('AM', 'Maître de Dossier', 'Maître de Dossier', 'Maître de Dossier', 'Transverse', 0),
	('HM', 'Chef de mission', 'Chef de mission', 'Chef de mission', 'ESC', 1),
	('AC', 'Expert comptable', 'Expert comptable', 'Expert comptable', 'ESC', 1),
	('PC', 'Collaborateur principal', 'Collaborateur principal', 'Collaborateur principal', 'ESC', 1),
	('SC', 'Collaborateur secondaire', 'Collaborateur secondaire', 'Collaborateur secondaire', 'ESC', 1),
	('AP', 'Partenaire conseil', 'Partenaire conseil', 'Partenaire conseil', 'Conseil', 1)
END

-- Migration des libellés pour les existants
UPDATE [account].[Label] SET [CustomerLabel] = 'Responsable de Compte', [CollaboratorLabel] = 'Responsable de Compte', [Description] = 'Responsable de Compte' WHERE [Code] = 'CLP' AND [CollaboratorLabel] <> 'Responsable de Compte';
UPDATE [account].[Label] SET [CustomerLabel] = 'Maître de Dossier', [CollaboratorLabel] = 'Maître de Dossier', [Description] = 'Maître de Dossier' WHERE [Code] = 'AM' AND [CollaboratorLabel] <> 'Maître de Dossier';