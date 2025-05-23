/*
Modèle de script de post-déploiement
--------------------------------------------------------------------------------------
 Ce fichier contient des instructions SQL qui seront ajoutées au script de compilation.
 Utilisez la syntaxe SQLCMD pour inclure un fichier dans le script de post-déploiement.
 Exemple :      :r .\monfichier.sql
 Utilisez la syntaxe SQLCMD pour référencer une variable dans le script de post-déploiement.
 Exemple :      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/
CREATE OR ALTER PROCEDURE [account].[InsertOfficeIfNotExists] 
    @OfficeName NVARCHAR(255),
    @PhoneNumber NVARCHAR(50),
    @AddressLine1 NVARCHAR(255),
    @ZipCode NVARCHAR(20),
    @City NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Result INT;
    DECLARE @AddressId INT;
    SET @Result = 0;
    SELECT @AddressId = AddressId
    FROM [account].[Address]
    WHERE AddressLine1 = @AddressLine1
      AND ZipCode = @ZipCode
      AND City = @City;

    IF @AddressId IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM [account].[Office] WHERE [Name] = @OfficeName
    )
    BEGIN
        INSERT INTO [account].[Office] ([Name], [PhoneNumber], [AddressId])
        VALUES (@OfficeName, @PhoneNumber, @AddressId);
        SET @Result = 1;
        RETURN;
    END

    IF @Result = 1
        PRINT 'Office inserted successfully.';
    ELSE
        PRINT 'Office already exists or address not found.';
END
GO

-- Example usage
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Juillan - Tarbes', @PhoneNumber = '05 62 34 78 15', @AddressLine1 = 'Téléport 7', @ZipCode = '65290', @City = 'Juillan';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Menton', @PhoneNumber = '04 93 35 48 83', @AddressLine1 = '6 rue Frères Picco', @ZipCode = '06500', @City = 'Menton';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Fort de France', @PhoneNumber = '05 96 71 92 72', @AddressLine1 = 'Quartiers de Dillon Stade', @ZipCode = '97200', @City = 'Fort-de-France';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Le Havre', @PhoneNumber = '0232747600', @AddressLine1 = '46 rue Louis Eudier', @ZipCode = '76600', @City = 'Le Havre';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Dijon', @PhoneNumber = '03 80 78 86 10', @AddressLine1 = '6 rue Paul Verlaine', @ZipCode = '21000', @City = 'Dijon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Pont-Audemer', @PhoneNumber = '02 32 41 51 34', @AddressLine1 = '8 rue du Président Georges Pompidou', @ZipCode = '27500', @City = 'Pont-Audemer';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saintes', @PhoneNumber = '05 46 74 29 91', @AddressLine1 = '2 chemin des Marsais ', @ZipCode = '17100', @City = 'Saintes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lyon', @PhoneNumber = '04 37 64 75 00', @AddressLine1 = '21 rue Antonin Laborde', @ZipCode = '69009', @City = 'Lyon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Tours', @PhoneNumber = '02 47 63 47 00', @AddressLine1 = '2 Allée Colette Duval Bât J', @ZipCode = '37100', @City = 'Tours';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Sens', @PhoneNumber = '03 86 64 03 98', @AddressLine1 = 'ECO PARC, 49 Rue du 19 Mars 1962', @ZipCode = '89100', @City = 'Sens';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Sauveur - Luxeuil-les-Bains', @PhoneNumber = '03 84 93 78 22', @AddressLine1 = '66 rue Edouard Herriot', @ZipCode = '70300', @City = 'Saint-Sauveur';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Mantes-la-Jolie', @PhoneNumber = '01 34 00 15 15', @AddressLine1 = '48 avenue République', @ZipCode = '78200', @City = 'Mantes-la-Jolie';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Creysse - Bergerac', @PhoneNumber = '05 53 61 72 24', @AddressLine1 = '32 ZA La Nauve Nord', @ZipCode = '24100', @City = 'Creysse';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Epernay', @PhoneNumber = '03 26 51 16 26', @AddressLine1 = '2 allée Côte des Blancs', @ZipCode = '51200', @City = 'Epernay';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Gensac-la-Pallue - Cognac', @PhoneNumber = '05 45 35 13 01', @AddressLine1 = '27 route de la Grue', @ZipCode = '16130', @City = 'Gensac-la-Pallue';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Nantes', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '7 boulevard Albert Einstein', @ZipCode = '44300', @City = 'Nantes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Dié-des-Vosges', @PhoneNumber = '03 29 55 27 08', @AddressLine1 = '11 Parc d''activités', @ZipCode = '88470', @City = 'Saint-Michel-sur-Meurthe';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Coutances', @PhoneNumber = '02 33 45 42 62', @AddressLine1 = '9000 rue de la nouvelle idée', @ZipCode = '50200', @City = 'Coutances';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montceau-les-Mines', @PhoneNumber = '03 85 57 06 06', @AddressLine1 = '15 rue Carnot', @ZipCode = '71300', @City = 'Montceau-les-Mines';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Châteauroux', @PhoneNumber = '02 54 22 03 31', @AddressLine1 = '3 Place Colbert', @ZipCode = '36003', @City = 'Châteauroux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Vire', @PhoneNumber = '02 31 67 73 12', @AddressLine1 = '13 rue Emile Zimmermann', @ZipCode = '14500', @City = 'Vire';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lisieux', @PhoneNumber = '02 31 31 33 02', @AddressLine1 = '94 Route de Cormeilles', @ZipCode = '14100', @City = 'Lisieux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Angoulême', @PhoneNumber = '05 45 90 37 00', @AddressLine1 = '144 Route de Vars', @ZipCode = '16160', @City = 'Gond-Pontouvre';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Le Mans', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '40 place République', @ZipCode = '72000', @City = 'Le Mans';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cayenne', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '8 rue des Bourdons', @ZipCode = '97300', @City = 'Cayenne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Mulhouse', @PhoneNumber = '03 89 32 94 94', @AddressLine1 = '60 rue Jacques Mugnier', @ZipCode = '68100', @City = 'Mulhouse';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Mérignac - Bordeaux', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '11 rue Archimède', @ZipCode = '33700', @City = 'Mérignac';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Beauvais', @PhoneNumber = '03 44 05 46 46', @AddressLine1 = '8 avenue du Beauvaisis', @ZipCode = '60000', @City = 'Beauvais';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bois Guillaume - Rouen', @PhoneNumber = '02 35 52 68 60', @AddressLine1 = '71 avenue Antoine de Saint Exupéry', @ZipCode = '76230', @City = 'Bois Guillaume';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Etampes', @PhoneNumber = '01 64 94 38 18', @AddressLine1 = '1 rue des Epinants', @ZipCode = '91150', @City = 'Etampes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Pontarlier', @PhoneNumber = '03 81 46 28 50', @AddressLine1 = '1 rue Hélène Boucher', @ZipCode = '25300', @City = 'Pontarlier';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Alençon', @PhoneNumber = '02 33 82 33 00', @AddressLine1 = '22 bis rue Villeneuve', @ZipCode = '61000', @City = 'Alençon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Pissotte - Fontenay-le-Compte', @PhoneNumber = '02 51 69 95 10', @AddressLine1 = 'D938T', @ZipCode = '85200', @City = 'Pissotte';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'La Teste-de-Buch', @PhoneNumber = '05 57 52 33 50', @AddressLine1 = '3B avenue Binghamton', @ZipCode = '33260', @City = 'La Teste-de-Buch';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Louhans', @PhoneNumber = '03 85 75 71 11', @AddressLine1 = '41 rue Colombier', @ZipCode = '71500', @City = 'Louhans';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Marmande', @PhoneNumber = '05 53 20 35 10', @AddressLine1 = '10 rue Arago', @ZipCode = '47200', @City = 'Marmande';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Yquelon - Granville', @PhoneNumber = '02 33 50 07 76', @AddressLine1 = '132 Rue Bocagère', @ZipCode = '50400', @City = 'Yquelon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Limoges', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '25 rue Hubert Curien', @ZipCode = '87000', @City = 'Limoges';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Rezé', @PhoneNumber = '02 28 07 05 00', @AddressLine1 = '2 rue Joseph Conrad', @ZipCode = '44400', @City = 'Rezé';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Avold', @PhoneNumber = '03 87 91 11 43', @AddressLine1 = '5 rue de la Piscine', @ZipCode = '57500', @City = 'Saint-Avold';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bessines - Niort', @PhoneNumber = '05 49 73 55 55', @AddressLine1 = '1 rue du Champ de la Motte', @ZipCode = '79000', @City = 'Bessines';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Germain-en-Laye', @PhoneNumber = '01 39 73 73 64', @AddressLine1 = '3 place André Malraux', @ZipCode = '78100', @City = 'Saint-Germain-en-Laye';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lens', @PhoneNumber = '03 21 14 71 50', @AddressLine1 = '19 rue Diderot', @ZipCode = '62300', @City = 'Lens';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montélimar', @PhoneNumber = '04 75 92 01 80', @AddressLine1 = '114 route de Châteauneuf', @ZipCode = '26200', @City = 'Montélimar';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Hérouville Saint Clair - Caen', @PhoneNumber = '02 14 37 55 00', @AddressLine1 = '5 avenue Dubna', @ZipCode = '14200', @City = 'Hérouville Saint Clair';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Nevers', @PhoneNumber = '03 86 71 64 00', @AddressLine1 = '72 rue Marzy', @ZipCode = '58000', @City = 'Nevers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Alès', @PhoneNumber = '04 66 30 72 40', @AddressLine1 = '4 rue de la Bergerie', @ZipCode = '30100', @City = 'Alès';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Quimperlé', @PhoneNumber = '02 98 96 23 76', @AddressLine1 = '46 B rue Eric Tarbarly', @ZipCode = '29300', @City = 'Quimperlé';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bourges', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '14 allée Charles Pathé', @ZipCode = '18000', @City = 'Bourges';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Metz', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '2 rue Pierre Simon de Laplace', @ZipCode = '57070', @City = 'Metz';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Péronne', @PhoneNumber = '03 22 73 38 00', @AddressLine1 = '29 rue St Sauveur', @ZipCode = '80200', @City = 'Péronne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montargis', @PhoneNumber = '02 38 95 00 72', @AddressLine1 = '47 rue Jean Jaurès', @ZipCode = '45200', @City = 'Montargis';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montluçon', @PhoneNumber = '04 70 08 22 70', @AddressLine1 = '3 avenue Marx Dormoy', @ZipCode = '03100', @City = 'Montluçon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montbéliard', @PhoneNumber = '03 81 91 15 64', @AddressLine1 = '1655 Allée Henri Hugoniot', @ZipCode = '25600', @City = 'Brognard';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Malo', @PhoneNumber = '02 23 18 00 35', @AddressLine1 = '25 rue de l''Arkansas', @ZipCode = '35400', @City = 'Saint-Malo';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Brive-la-Gaillarde', @PhoneNumber = '05 55 17 06 00', @AddressLine1 = '34 bis Avenue Alsace Lorraine', @ZipCode = '19100', @City = 'Brive-la-Gaillarde';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'La Roche-sur-Yon', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '14 rue Montesquieu', @ZipCode = '85000', @City = 'La Roche-sur-Yon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bitche', @PhoneNumber = '03 87 96 05 14', @AddressLine1 = '3 avenue Gén de Gaulle', @ZipCode = '57230', @City = 'Bitche';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Nice', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '51 avenue Simone Veil', @ZipCode = '06200', @City = 'Nice';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Gérardmer', @PhoneNumber = '03 29 63 36 78', @AddressLine1 = '55 rue François Mitterrand', @ZipCode = '88400', @City = 'Gérardmer';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Grégoire - Rennes', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = 'rue Terre Victoria', @ZipCode = '35760', @City = 'Saint-Grégoire';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bayonne - Biarritz / Anglet', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '52 Avenue du 8 mai 1945', @ZipCode = '64100', @City = 'Bayonne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Sarrebourg', @PhoneNumber = '03 87 03 19 38', @AddressLine1 = '6 Terrasse Normandie', @ZipCode = '57400', @City = 'Sarrebourg';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Gien', @PhoneNumber = '02 38 05 11 20', @AddressLine1 = '49 avenue Chantemerle', @ZipCode = '45500', @City = 'Gien';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Roanne', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '39 rue Jean Moulin', @ZipCode = '42300', @City = 'Roanne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Nancy', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '35 avenue 20ème Corps', @ZipCode = '54000', @City = 'Nancy';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Jean-du-Falga - Pamiers', @PhoneNumber = '05 34 01 33 83', @AddressLine1 = '2 rue Mille Hommes', @ZipCode = '09100', @City = 'Saint-Jean-du-Falga';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Nîmes', @PhoneNumber = '04 66 68 91 91', @AddressLine1 = '308 allée de l''Amérique Latine', @ZipCode = '30900', @City = 'Nîmes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Marcq-en-Baroeul - Lille', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '36 rue Eugène Jacquet', @ZipCode = '59700', @City = 'Marcq-en-Baroeul';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Louviers', @PhoneNumber = '02 32 25 26 10', @AddressLine1 = '2 boulevard Maréchal Joffre', @ZipCode = '27400', @City = 'Louviers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Puget-sur-Argens', @PhoneNumber = '04 94 19 68 41', @AddressLine1 = '76 boulevard Progrès', @ZipCode = '83480', @City = 'Puget-sur-Argens';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Chambéry', @PhoneNumber = '04 79 62 99 11', @AddressLine1 = '24 rue Aristide Bergès', @ZipCode = '73000', @City = 'Chambéry';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Blois', @PhoneNumber = '02 54 90 26 60', @AddressLine1 = '21 rue de la Vallée Maillard', @ZipCode = '41043', @City = 'Blois';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Albi', @PhoneNumber = '05 63 48 75 10', @AddressLine1 = 'ZAC Les Portes d''Albi', @ZipCode = '81000', @City = 'Albi';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Baie-Mahault', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = 'Les Galeries de Houelbourg', @ZipCode = '97122', @City = 'Baie-Mahault';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Abbeville', @PhoneNumber = '03 22 99 90 00', @AddressLine1 = 'ZAC des 2 Vallées', @ZipCode = '80100', @City = 'Abbeville';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montévrain', @PhoneNumber = '01 71 58 02 20', @AddressLine1 = '1 rue de Berlin', @ZipCode = '77144', @City = 'MONTEVRAIN';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Creil', @PhoneNumber = '03 44 55 97 00', @AddressLine1 = '2 allée de la Forêt d''Halatte', @ZipCode = '60100', @City = 'Creil';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montpellier', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '251 rue Euclide', @ZipCode = '34000', @City = 'Montpellier';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Labège - Toulouse', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '224 rue Carmin', @ZipCode = '31670', @City = 'Labège';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Sarreguemines', @PhoneNumber = '03 87 95 61 87', @AddressLine1 = '31 rue Poincaré', @ZipCode = '57200', @City = 'Sarreguemines';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Charleville-Mézières', @PhoneNumber = '03 24 33 25 57', @AddressLine1 = '6 place de la Gare', @ZipCode = '08000', @City = 'Charleville-Mézières';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Morteau', @PhoneNumber = '03 81 67 19 23', @AddressLine1 = '8 avenue Gén Charles de Gaulle', @ZipCode = '25500', @City = 'Morteau';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lons-le-Saunier', @PhoneNumber = '03 84 87 15 45', @AddressLine1 = '17 place Verdun', @ZipCode = '39000', @City = 'Lons-le-Saunier';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Auxerre', @PhoneNumber = '03 86 72 90 70', @AddressLine1 = '1 avenue St Georges', @ZipCode = '89000', @City = 'Auxerre';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montigny-le-Bretonneux - Versailles / Saint-Quentin', @PhoneNumber = '01 39 30 53 00', @AddressLine1 = '60 avenue Centre', @ZipCode = '78180', @City = 'Montigny-le-Bretonneux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Guéret', @PhoneNumber = '05 55 52 08 55', @AddressLine1 = '12 avenue Berry', @ZipCode = '23000', @City = 'Guéret';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Châlons-en-Champagne', @PhoneNumber = '03 26 65 17 15', @AddressLine1 = '14 avenue du Général George Smith Patton', @ZipCode = '51000', @City = 'Châlons-en-Champagne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Ambert', @PhoneNumber = '04 73 82 22 44', @AddressLine1 = '3 avenue de la Dore', @ZipCode = '63600', @City = 'Ambert';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Thyez - Cluses', @PhoneNumber = '04 50 18 23 03', @AddressLine1 = '156 rue Sorbiers', @ZipCode = '74300', @City = 'Thyez';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Castres', @PhoneNumber = '05 63 71 82 00', @AddressLine1 = '17 rue Léon Blum', @ZipCode = '81100', @City = 'Castres';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Maisons-Laffitte', @PhoneNumber = '01 61 04 72 00', @AddressLine1 = '4 Avenue de Saint-Germain', @ZipCode = '78600', @City = 'Maisons-Laffitte';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Maubeuge', @PhoneNumber = '03 27 62 48 88', @AddressLine1 = '48 boulevard de l''Europe', @ZipCode = '59600', @City = 'Maubeuge';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Digoin', @PhoneNumber = '03 85 88 55 80', @AddressLine1 = '6 rue Charmes', @ZipCode = '71160', @City = 'Digoin';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Roubaix', @PhoneNumber = '03 28 45 90 89', @AddressLine1 = '1 Grande Rue', @ZipCode = '59100', @City = 'Roubaix';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Dax', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '178 avenue St Vincent de Paul', @ZipCode = '40100', @City = 'Dax';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Angers', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '12 rue Papiau de la Verrie', @ZipCode = '49000', @City = 'Angers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Quentin', @PhoneNumber = '03 23 05 78 80', @AddressLine1 = '50 rue de Baudreuil', @ZipCode = '02100', @City = 'Saint-Quentin';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Brest', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '12 quai Armand Considère', @ZipCode = '29200', @City = 'Brest';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Paris La Défense - Courbevoie', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '2 avenue Gambetta', @ZipCode = '92400', @City = 'Courbevoie';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Orléans', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '14 avenue des Droits de l''homme', @ZipCode = '45100', @City = 'Orléans';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Aurillac', @PhoneNumber = '04 71 45 49 00', @AddressLine1 = '38 bis avenue Georges Pompidou', @ZipCode = '15000', @City = 'Aurillac';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Neuville-lès-Dieppe', @PhoneNumber = '02 35 82 87 21', @AddressLine1 = '32 rue Louis Blériot', @ZipCode = '76370', @City = 'Neuville-lès-Dieppe';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Moulins', @PhoneNumber = '04 70 46 37 03', @AddressLine1 = '24 rue des Tanneries', @ZipCode = '03000', @City = 'Moulins';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Boulogne-sur-Mer', @PhoneNumber = '03 21 33 48 58', @AddressLine1 = '1 boulevard Auguste Mariette', @ZipCode = '62200', @City = 'Boulogne-sur-Mer';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Beausoleil', @PhoneNumber = '04 93 41 79 79', @AddressLine1 = '33 boulevard Gén Leclerc', @ZipCode = '06240', @City = 'Beausoleil';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Troyes', @PhoneNumber = '03 25 82 65 00', @AddressLine1 = '42 rue Paix', @ZipCode = '10000', @City = 'Troyes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Hauts-de-Bienne - Morez', @PhoneNumber = '03 84 33 10 57', @AddressLine1 = '194 rue République', @ZipCode = '39400', @City = 'Hauts-de-Bienne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Graulhet', @PhoneNumber = '05 63 42 22 30', @AddressLine1 = '20 boulevard Georges Ravari', @ZipCode = '81300', @City = 'Graulhet';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Deauville', @PhoneNumber = '02 31 88 49 45', @AddressLine1 = '1A rue Victor Hugo', @ZipCode = '14800', @City = 'Deauville';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'La-Ferté-sous-Jouarre', @PhoneNumber = '01 60 22 20 06', @AddressLine1 = '15 rue Merlette', @ZipCode = '77260', @City = 'La-Ferté-sous-Jouarre';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cholet', @PhoneNumber = '02 41 49 53 00', @AddressLine1 = '28 rue Terre-Neuve', @ZipCode = '49300', @City = 'Cholet';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Arras', @PhoneNumber = '03 21 24 38 40', @AddressLine1 = '3/5 Grand''Place', @ZipCode = '62000', @City = 'Arras';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Clermont-Ferrand', @PhoneNumber = '04 73 44 70 65', @AddressLine1 = '8 rue Eric de Cromières', @ZipCode = '63000', @City = 'Clermont-Ferrand';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Yutz - Thionville', @PhoneNumber = '03 82 86 00 00', @AddressLine1 = '5 rue Lorraine', @ZipCode = '57970', @City = 'Yutz';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-André', @PhoneNumber = '02 62 46 01 12', @AddressLine1 = '665 rue de la gare', @ZipCode = '97440', @City = 'Saint-André';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Perpignan', @PhoneNumber = '04 68 66 43 70', @AddressLine1 = '1098 avenue Eole', @ZipCode = '66000', @City = 'Perpignan';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Denis', @PhoneNumber = '02 62 94 84 24', @AddressLine1 = '4 rue Camille Vergoz', @ZipCode = '97400', @City = 'Saint-Denis';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Manosque', @PhoneNumber = '04 92 71 74 20', @AddressLine1 = '79 avenue Mar De Lattre de Tassigny', @ZipCode = '04100', @City = 'Manosque';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Coulounieix Chamiers - Périgueux', @PhoneNumber = '05 53 03 54 22', @AddressLine1 = '445 boulevard des saveurs', @ZipCode = '24660', @City = 'Coulounieix Chamiers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Frangy', @PhoneNumber = '04 50 44 82 85', @AddressLine1 = '685 rue Grand Pont', @ZipCode = '74270', @City = 'Frangy';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Mâcon', @PhoneNumber = '03 85 21 51 00', @AddressLine1 = '106 rue du Km 400', @ZipCode = '71000', @City = 'Mâcon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bourgoin-Jallieu', @PhoneNumber = '04 74 93 24 09', @AddressLine1 = '1 allée Claude Chappe', @ZipCode = '38300', @City = 'Bourgoin-Jallieu';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Hazebrouck', @PhoneNumber = '03 28 44 22 30', @AddressLine1 = '80 boulevard de l''Abbé Lemire', @ZipCode = '59190', @City = 'Hazebrouck';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Thiers', @PhoneNumber = '04 73 80 23 86', @AddressLine1 = '60 avenue Léo Lagrange', @ZipCode = '63300', @City = 'Thiers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Rodez', @PhoneNumber = '05 65 77 21 60', @AddressLine1 = '42 rue Docteur Théodore Mathieu', @ZipCode = '12000', @City = 'Rodez';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Haguenau', @PhoneNumber = '03 88 93 08 81', @AddressLine1 = '95 route Marienthal', @ZipCode = '67500', @City = 'Haguenau';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Dunkerque', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '124 rue du magasin général', @ZipCode = '59140', @City = 'Dunkerque';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Narbonne', @PhoneNumber = '04 68 65 40 60', @AddressLine1 = 'avenue du Forum', @ZipCode = '11100', @City = 'Narbonne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Villeneuve-de-Rivière - Saint Gaudens', @PhoneNumber = '05 62 00 80 00', @AddressLine1 = '34 rue du Moulin d''Aulne', @ZipCode = '31800', @City = 'Villeneuve-de-Rivière';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montauban', @PhoneNumber = '05 63 91 70 50', @AddressLine1 = '240 avenue Espagne', @ZipCode = '82000', @City = 'Montauban';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cergy', @PhoneNumber = '01 34 22 94 20', @AddressLine1 = '4/6 rue des Chauffours', @ZipCode = '95000', @City = 'CERGY';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Marseille', @PhoneNumber = '04 96 20 53 60', @AddressLine1 = '132 Boulevard Michelet', @ZipCode = '13272', @City = 'Marseille';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Boé - Agen', @PhoneNumber = '05 53 77 59 00', @AddressLine1 = '1 rue Albert Ferrasse', @ZipCode = '47550', @City = 'Boé';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Davezieux - Annonay', @PhoneNumber = '04 75 33 73 60', @AddressLine1 = '57 rue Pins', @ZipCode = '07430', @City = 'Davezieux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Forbach', @PhoneNumber = '03 87 29 26 60', @AddressLine1 = '8 avenue St Rémy', @ZipCode = '57600', @City = 'Forbach';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Chalon-sur-Saône', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '32 quai St Cosme', @ZipCode = '71100', @City = 'Chalon-sur-Saône';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Chinon', @PhoneNumber = '02 47 93 10 18', @AddressLine1 = '8 quai Pasteur', @ZipCode = '37500', @City = 'Chinon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Fougères', @PhoneNumber = '02 99 17 20 00', @AddressLine1 = '1 rue Pellerine', @ZipCode = '35300', @City = 'Fougères';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Epinal', @PhoneNumber = '03 29 31 10 31', @AddressLine1 = '44 rue Léo Valentin', @ZipCode = '88000', @City = 'Epinal';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cahors', @PhoneNumber = '05 65 24 71 26', @AddressLine1 = '58 place de la Résistance', @ZipCode = '46000', @City = 'Cahors';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Valenciennes', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '1 rue de l''Hôpital de Siège', @ZipCode = '59300', @City = 'Valenciennes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Béthune', @PhoneNumber = '03 61 88 42 07', @AddressLine1 = '25 Rue Eugène Haynaut', @ZipCode = '62400', @City = 'Béthune';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Mont-de-Marsan', @PhoneNumber = '05 58 75 02 48', @AddressLine1 = 'Immeuble Office 31', @ZipCode = '40280', @City = 'Saint-Pierre-du-Mont';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Claude', @PhoneNumber = '03 84 33 63 70', @AddressLine1 = '8 rue Reybert', @ZipCode = '39200', @City = 'Saint-Claude';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Auch', @PhoneNumber = '05 62 60 64 40', @AddressLine1 = '152 Route d''Agen', @ZipCode = '32000', @City = 'Auch';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Toulon', @PhoneNumber = '04 94 18 90 70', @AddressLine1 = '270 Avenue Jean D''Ormesson Immeuble Le Nobel', @ZipCode = '83160', @City = 'LA VALETTE DU VAR';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Plérin - Saint-Brieuc', @PhoneNumber = '02 96 79 82 79', @AddressLine1 = '9 rue Hélène Boucher', @ZipCode = '22190', @City = 'Plérin';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Amiens', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '7 avenue du Danemark', @ZipCode = '80090', @City = 'Amiens';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Valbonne', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '291 rue Albert Caquot', @ZipCode = '06560', @City = 'Valbonne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Chartres', @PhoneNumber = '02 37 28 10 89', @AddressLine1 = '6 avenue Nicolas Conté', @ZipCode = '28000', @City = 'Chartres';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Reims', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '19 rue Clément Ader', @ZipCode = '51100', @City = 'Reims';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Lô', @PhoneNumber = '02 33 77 14 14', @AddressLine1 = '89 rue des Cinq Chemins', @ZipCode = '50000', @City = 'Saint-Lô';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Paris - Liège', @PhoneNumber = '01 40 82 19 63', @AddressLine1 = '36 rue de Liège', @ZipCode = '75008', @City = 'Paris';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Challans', @PhoneNumber = '02 51 49 34 47', @AddressLine1 = '16 rue Owen Chamberlain', @ZipCode = '85300', @City = 'Challans';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Meaux', @PhoneNumber = '01 60 25 18 32', @AddressLine1 = '18 avenue du Président Salvador Allende', @ZipCode = '77100', @City = 'Meaux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Le Creusot', @PhoneNumber = '03 85 78 87 87', @AddressLine1 = '90B allée Hubert Curien', @ZipCode = '71200', @City = 'Le Creusot';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Longwy', @PhoneNumber = '03 82 23 00 00', @AddressLine1 = '38 rue Legendre', @ZipCode = '54400', @City = 'Longwy';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Evreux', @PhoneNumber = '02 32 28 19 60', @AddressLine1 = '67 rue Pierre Tal Coat', @ZipCode = '27000', @City = 'Evreux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Annecy le Vieux', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '15 rue Pré-Paillard', @ZipCode = '74940', @City = 'Annecy le Vieux';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Reichstett - Strasbourg', @PhoneNumber = '03 88 18 23 00', @AddressLine1 = '200 rue de Paris', @ZipCode = '67116', @City = 'Reichstett';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Quimper', @PhoneNumber = '02 98 64 54 00', @AddressLine1 = '9 rue Président Sadate', @ZipCode = '29000', @City = 'Quimper';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Nazaire', @PhoneNumber = '02 40 45 80 28', @AddressLine1 = '43 boulevard Université', @ZipCode = '44600', @City = 'Saint-Nazaire';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Valence', @PhoneNumber = '04 75 41 89 89', @AddressLine1 = '49 Avenue des Langories', @ZipCode = '26000', @City = 'Valence';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Marcellin', @PhoneNumber = '04 76 64 94 64', @AddressLine1 = '44 cours Vallier', @ZipCode = '38160', @City = 'Saint-Marcellin';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cherbourg - Octeville', @PhoneNumber = '02 33 88 36 38', @AddressLine1 = '28 avenue Mar de Lattre de Tassigny', @ZipCode = '50100', @City = 'Cherbourg-Octeville';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lagord - La Rochelle', @PhoneNumber = '05 46 50 57 67', @AddressLine1 = '4 rue Louis Tardy', @ZipCode = '17140', @City = 'Lagord';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Vitré', @PhoneNumber = '02 23 55 13 60', @AddressLine1 = '8 rue Epinettes', @ZipCode = '35500', @City = 'Vitré';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Besançon', @PhoneNumber = '03 81 41 70 10', @AddressLine1 = '17 Avenue des Montboucons', @ZipCode = '25000', @City = 'Besançon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Pau', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '4 boulevard Lucien Favre', @ZipCode = '64000', @City = 'Pau';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Cambrai', @PhoneNumber = '03 27 82 95 10', @AddressLine1 = '20-22 rue du Maréchal de Lattre De Tassigny', @ZipCode = '59400', @City = 'Cambrai';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Etienne', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '4 allée Drouot', @ZipCode = '42100', @City = 'Saint-Etienne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Sète', @PhoneNumber = '04 67 46 65 10', @AddressLine1 = 'Espace Don Quichotte,', @ZipCode = '34200', @City = 'Sète';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Longuenesse - Saint-Omer', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '47 avenue Clemenceau', @ZipCode = '62219', @City = 'Longuenesse';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Douai', @PhoneNumber = '03 27 88 91 58', @AddressLine1 = '39 rue Mongat', @ZipCode = '59500', @City = 'Douai';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Remiremont', @PhoneNumber = '03 29 26 29 26', @AddressLine1 = '16 rue des Cardes', @ZipCode = '88200', @City = 'Remiremont';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Carcassonne', @PhoneNumber = '04 68 25 91 64', @AddressLine1 = '1876 boulevard François Xavier Faffeur', @ZipCode = '11000', @City = 'Carcassonne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Vannes', @PhoneNumber = '02 97 63 13 73', @AddressLine1 = '1 rue Anita Conti', @ZipCode = '56000', @City = 'Vannes';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Jean-de-Luz', @PhoneNumber = '05 59 51 58 60', @AddressLine1 = '24 zone industrielle Layats', @ZipCode = '64500', @City = 'Saint-Jean-de-Luz';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Calais', @PhoneNumber = '03 21 46 74 44', @AddressLine1 = '10 Boulevard du Parc', @ZipCode = '62231', @City = 'Coquelles';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Laval', @PhoneNumber = '02 43 59 06 40', @AddressLine1 = '7 rue Paradis', @ZipCode = '53000', @City = 'Laval';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Lorient', @PhoneNumber = '02 97 83 44 32', @AddressLine1 = '1 rue Honoré d''Estienne d''Orves', @ZipCode = '56100', @City = 'Lorient';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Montreuil-sur-Mer', @PhoneNumber = '03 61 22 10 01', @AddressLine1 = '7 Place Gambetta', @ZipCode = '62170', @City = 'Montreuil-sur-Mer';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Compiègne', @PhoneNumber = '03 44 55 97 00', @AddressLine1 = '114 Rue Saint Lazare', @ZipCode = '60200', @City = 'Compiègne';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'La Lande-Patry - Flers', @PhoneNumber = '02 33 65 06 60', @AddressLine1 = '8 Rue Denys Boudard', @ZipCode = '61100', @City = 'La Lande-Patry';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Bourg-en-Bresse', @PhoneNumber = '04 74 50 31 80', @AddressLine1 = '24 boulevard Jules Ferry', @ZipCode = '01000', @City = 'Bourg-en-Bresse';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Poitiers', @PhoneNumber = '05 49 38 45 50', @AddressLine1 = '7 rue Eugène Chevreul', @ZipCode = '86000', @City = 'Poitiers';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Meylan - Grenoble', @PhoneNumber = 'Numéro non trouvé', @AddressLine1 = '51 Chemin de la Taillat', @ZipCode = '38240', @City = 'Meylan';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Colmar', @PhoneNumber = '03 89 21 73 21', @AddressLine1 = '4E avenue du Général de Gaulle', @ZipCode = '68000', @City = 'Colmar';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Avignon', @PhoneNumber = '04 90 81 17 30', @AddressLine1 = '1335 Route de l''Aérodrome', @ZipCode = '84140', @City = 'Avignon';
EXEC [account].[InsertOfficeIfNotExists] @OfficeName = 'Saint-Girons', @PhoneNumber = '05 34 14 39 20', @AddressLine1 = '16 rue du Quai', @ZipCode = '09200', @City = 'Saint-Girons';

DROP PROCEDURE [account].[InsertOfficeIfNotExists];