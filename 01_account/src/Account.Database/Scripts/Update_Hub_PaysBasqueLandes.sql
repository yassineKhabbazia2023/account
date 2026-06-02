-- =============================================================================
-- Mise à jour du nom du hub pour correspondre au nom Pennylane
--   Nom plateforme : Pays Basque Landes
--   Nom Pennylane  : Pyrénées Basque Landes
-- Objectif : aligner le nom dans la plateforme avec celui de Pennylane.
-- =============================================================================
BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE h
    SET HubName = N'Pyrénées Basque Landes'
    FROM account.Hub h
    WHERE LTRIM(RTRIM(h.HubName)) = N'Pays Basque Landes';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
