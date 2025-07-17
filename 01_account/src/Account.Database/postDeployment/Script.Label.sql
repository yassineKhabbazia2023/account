IF NOT EXISTS (SELECT 1 FROM [account].[Label] WHERE [Code] = 'CLP')
BEGIN
	INSERT INTO [account].[Label] ([Code], [CustomerLabel], [CollaboratorLabel], [Description], [Business], [IsVisible])
	VALUES ('CLP', 'Responsable', 'CLP', 'Client Lead Partner', 'Transverse', 0),
	('AM', 'Chargé de mission', 'AM', 'Account manager', 'Transverse', 0),
	('HM', 'Chef de mission', 'Chef de mission', 'Chef de mission', 'ESC', 1),
	('AC', 'Expert comptable', 'Expert comptable', 'Expert comptable', 'ESC', 1),
	('PC', 'Collaborateur principal', 'Collaborateur principal', 'Collaborateur principal', 'ESC', 1),
	('SC', 'Collaborateur secondaire', 'Collaborateur secondaire', 'Collaborateur secondaire', 'ESC', 1),
	('AP', 'Partenaire conseil', 'Partenaire conseil', 'Partenaire conseil', 'Conseil', 1)
END