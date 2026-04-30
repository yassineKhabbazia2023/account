-- Pilote prise de RDV : passage en relation client par ville.
-- Met IsCustomerRelation a 1 et ActionLevel a 4 sur les Roles existants
-- des collabs listes ci-dessous quand l'adresse Delivery du compte
-- correspond a une des villes.

USE [account];
GO

SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Pilote TABLE (
    CollaboratorEmail VARCHAR(255) NOT NULL,
    City NVARCHAR(255) NOT NULL
);

INSERT INTO @Pilote VALUES
('jeremiegeorges@rydge.fr', N'Nancy'),
('jeremiegeorges@rydge.fr', N'Saint-Dié-des-Vosges'),
('jeremiegeorges@rydge.fr', N'Épinal'),
('jeremiegeorges@rydge.fr', N'Gérardmer'),
('jeremiegeorges@rydge.fr', N'Remiremont'),
('lcollot@rydge.fr', N'Bourges'),
('lcollot@rydge.fr', N'Châteauroux'),
('lcollot@rydge.fr', N'Nevers'),
('aleca@rydge.fr', N'Nancy'),
('aleca@rydge.fr', N'Saint-Dié-des-Vosges'),
('aleca@rydge.fr', N'Épinal'),
('aleca@rydge.fr', N'Gérardmer'),
('aleca@rydge.fr', N'Remiremont'),
('ghoutte@rydge.fr', N'Arras'),
('ghoutte@rydge.fr', N'Béthune'),
('ghoutte@rydge.fr', N'Cambrai'),
('ghoutte@rydge.fr', N'Douai'),
('ghoutte@rydge.fr', N'Lens'),
('ghoutte@rydge.fr', N'Maubeuge'),
('ghoutte@rydge.fr', N'Valenciennes'),
('colinemuller@rydge.fr', N'Colmar'),
('colinemuller@rydge.fr', N'Montbéliard'),
('colinemuller@rydge.fr', N'Mulhouse'),
('sabrinabrun@rydge.fr', N'Bourges'),
('sabrinabrun@rydge.fr', N'Châteauroux'),
('sabrinabrun@rydge.fr', N'Nevers'),
('ylegout@rydge.fr', N'Paris'),
('jbeugnot@rydge.fr', N'Montpellier'),
('jbeugnot@rydge.fr', N'Perpignan'),
('jbeugnot@rydge.fr', N'Carcassonne'),
('jbeugnot@rydge.fr', N'Narbonne'),
('jbeugnot@rydge.fr', N'Nîmes'),
('adeleurence@rydge.fr', N'Lyon'),
('adeleurence@rydge.fr', N'Bourg en Bresse'),
('adeleurence@rydge.fr', N'Bourgoin Jallieu'),
('adeleurence@rydge.fr', N'Morez'),
('adeleurence@rydge.fr', N'St-Claude'),
('dcaridroit@rydge.fr', N'Angers'),
('dcaridroit@rydge.fr', N'Le Mans'),
('dcaridroit@rydge.fr', N'Laval'),
('dcaridroit@rydge.fr', N'Cholet'),
('maubiniere@rydge.fr', N'Angers'),
('maubiniere@rydge.fr', N'Le Mans'),
('maubiniere@rydge.fr', N'Laval'),
('maubiniere@rydge.fr', N'Cholet');

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE r
    SET r.IsCustomerRelation = 1,
        r.ActionLevel = 4
    FROM account.[Role] r
    INNER JOIN actor.Contact c ON c.ContactId = r.ContactId
    INNER JOIN account.Address ad ON ad.AccountId = r.AccountId AND ad.AddressType = 'Delivery'
    INNER JOIN @Pilote p
        ON c.Email COLLATE French_CI_AI = p.CollaboratorEmail COLLATE French_CI_AI
        AND ad.City COLLATE French_CI_AI = p.City COLLATE French_CI_AI
    WHERE r.IsCustomerRelation IS NULL OR r.IsCustomerRelation = 0;

    PRINT 'Roles mis a jour : ' + CAST(@@ROWCOUNT AS VARCHAR);

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
