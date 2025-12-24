BEGIN TRY
    BEGIN TRANSACTION;

    --------------------------------------------------
    -- 1. Apply rename mapping A -> B
    --------------------------------------------------
    WITH Mapping AS (
        SELECT 'Amiens Haute Picardie' AS OldName, 'Amiens Somme Aisne' AS NewName UNION ALL
        SELECT 'Grand Ouest Parisien',   'Grande Couronne' UNION ALL
        SELECT 'Loire & Drôme Ardèche',  'ID&AL' UNION ALL
        SELECT 'Marne et Oise',          'Amiens Somme Aisne' UNION ALL
        SELECT 'Normandie Seine Baie de Somme', 'Amiens Somme Aisne' UNION ALL
        SELECT 'Toulouse Midi Pyrennées', 'Toulouse Midi Pyrénées' UNION ALL
        SELECT 'Paris',                  'Grande Couronne'
    ),
    Prep AS (
        SELECT
            'Hub ' + OldName AS FullOld,
            'Hub ' + NewName AS FullNew
        FROM Mapping
    )
    UPDATE h
    SET HubName = p.FullNew
    FROM account.Hub h
    JOIN Prep p
        ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(p.FullOld));


    --------------------------------------------------
    -- 2. Identify hub names impacted by the mapping
    --------------------------------------------------
    SELECT DISTINCT 'Hub ' + NewName AS HubName
    INTO #TargetHubNames
    FROM (VALUES
        ('Amiens Somme Aisne'),
        ('Grande Couronne'),
        ('ID&AL'),
        ('Toulouse Midi Pyrénées')
    ) v(NewName);


    --------------------------------------------------
    -- 3. Determine the hub to KEEP per name
    --------------------------------------------------
    SELECT
        h.HubName,
        MIN(h.HubId) AS KeepHubId
    INTO #HubToKeep
    FROM account.Hub h
    JOIN #TargetHubNames t
        ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(t.HubName))
    GROUP BY h.HubName;


    --------------------------------------------------
    -- 4. Identify duplicate hubs
    --------------------------------------------------
    SELECT
        h.HubId        AS DuplicateHubId,
        k.KeepHubId,
        h.HubName
    INTO #DuplicateHubs
    FROM account.Hub h
    JOIN #HubToKeep k
        ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(k.HubName))
    WHERE h.HubId <> k.KeepHubId;


    --------------------------------------------------
    -- 5. CAPTURE accounts to be updated (rollback trace)
    --------------------------------------------------
    SELECT
        a.AccountId,
        a.HubId            AS OldHubId,
        h.HubName          AS OldHubName,
        d.KeepHubId        AS NewHubId,
        hk.HubName         AS NewHubName
    INTO #UpdatedAccounts
    FROM account.Account a
    JOIN #DuplicateHubs d
        ON a.HubId = d.DuplicateHubId
    JOIN account.Hub h
        ON h.HubId = a.HubId
    JOIN account.Hub hk
        ON hk.HubId = d.KeepHubId;


    --------------------------------------------------
    -- 6. Reassign accounts → kept hub per name
    --------------------------------------------------
    UPDATE a
    SET HubId = d.KeepHubId
    FROM account.Account a
    JOIN #DuplicateHubs d
        ON a.HubId = d.DuplicateHubId;


    --------------------------------------------------
    -- 7. Delete duplicate hubs
    --------------------------------------------------
    DELETE FROM account.Hub
    WHERE HubId IN (
        SELECT DuplicateHubId FROM #DuplicateHubs
    );


    --------------------------------------------------
    -- 8. Optional: insert missing hub
    --------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM account.Hub
        WHERE HubName = 'Hub Rouen Seine Eure'
    )
    BEGIN
        INSERT INTO account.Hub (HubName)
        VALUES ('Hub Rouen Seine Eure');
    END;


    --------------------------------------------------
    -- 9. FINAL REPORT (rollback-ready)
    --------------------------------------------------
    SELECT
        AccountId,
        OldHubId,
        OldHubName,
        NewHubId,
        NewHubName
    FROM #UpdatedAccounts
    ORDER BY NewHubName, AccountId;


    --------------------------------------------------
    -- 10. Cleanup
    --------------------------------------------------
    DROP TABLE #TargetHubNames;
    DROP TABLE #HubToKeep;
    DROP TABLE #DuplicateHubs;
    DROP TABLE #UpdatedAccounts;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
