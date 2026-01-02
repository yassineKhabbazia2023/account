BEGIN TRY
    BEGIN TRANSACTION;

    --------------------------------------------------
    -- 1. Remove "Hub" prefix at start of HubName
    --------------------------------------------------
    UPDATE h
    SET HubName = LTRIM(SUBSTRING(h.HubName, 4, LEN(h.HubName)))
    FROM account.Hub h
    WHERE h.HubName LIKE 'Hub %';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
