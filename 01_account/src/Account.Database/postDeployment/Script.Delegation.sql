DECLARE @contactNexity2 int;

SET @contactNexity2 = (SELECT ContactId FROM actor.Contact WHERE ContactGlobalUniqueId = 'D6994F9A-017A-4144-AF47-F973AB517268');

INSERT INTO account.Delegation (DelegatorId, DelegateeId, StartDate, EndDate, Status, Note, CreationDate)
VALUES
(@contactRothschild, @contactRenault, CAST ('2019-02-11' AS datetime2), CAST ('2019-03-01' AS datetime2), 'disabled', 'Délégation spéciale', SYSDATETIME()),
(@contactAlstom1, @contactAlstom2, CAST ('1999-08-09' AS datetime2), CAST ('2000-02-21' AS datetime2), 'disabled', NULL, SYSDATETIME()),
(@contactAlstom1, @contactRenault, CAST ('2024-01-01' AS datetime2), NULL, 'enabled', 'Délégation long terme', SYSDATETIME()),
(@contactImagotag2, @contactImagotag1, CAST ('2022-07-01' AS datetime2), CAST ('2022-09-01' AS datetime2), 'disabled', 'Remplacement', SYSDATETIME()),
(@contactImagotag2, @contactImagotag1, CAST ('2024-05-17' AS datetime2), CAST ('2025-01-31' AS datetime2), 'pending', 'Demande de délégation', SYSDATETIME()),
(@contactFleury, @contactAlstom2, CAST ('2023-11-29' AS datetime2), NULL, 'pending', 'Je voudrais donner les droits à partir du 29 nov 2023 pour une durée indéterminée', SYSDATETIME()),
(@contactNexity, @contactNexity2, CAST ('2024-02-07' AS datetime2), CAST ('2024-04-01' AS datetime2), 'enabled', 'En cours', SYSDATETIME());