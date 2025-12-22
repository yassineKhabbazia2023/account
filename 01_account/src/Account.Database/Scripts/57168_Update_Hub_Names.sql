-- Action script
-- 1. Build mapping A -> B
WITH Mapping AS (
    SELECT 'Amiens Haute Picardie' AS OldName, 'Amiens Somme Aisne' AS NewName UNION ALL
    SELECT 'Grand Ouest Parisien',   'Grande Couronne' UNION ALL
    SELECT 'Loire&Drôme Ardèche',    'ID&AL' UNION ALL
    SELECT 'Marne et Oise',          'Amiens Somme Aisne' UNION ALL
    SELECT 'Normandie Seine Baie de Somme', 'Amiens Somme Aisne' UNION ALL
    SELECT 'Paris',                  'Grande Couronne'
),
Prep AS (
    SELECT
        'Hub ' + OldName AS FullOld,
        'Hub ' + NewName AS FullNew
    FROM Mapping
),
CheckExisting AS (
    SELECT
        p.FullOld,
        p.FullNew,
        h.HubName AS ExistingName
    FROM Prep p
    LEFT JOIN account.Hub h
        ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(p.FullOld))
)

-- 2. Materialize to reuse
SELECT *
INTO #CheckExisting
FROM CheckExisting;


--------------------------------------
-- 3. Update existing hubs
--------------------------------------
UPDATE h
SET HubName = c.FullNew
FROM account.Hub h
JOIN #CheckExisting c
    ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(c.FullOld));


--------------------------------------
-- 4. Show updated rows
--------------------------------------
SELECT DISTINCT
    'Updated' AS Status,
    FullOld AS OldValue,
    FullNew AS NewValue
FROM #CheckExisting
WHERE ExistingName IS NOT NULL;


--------------------------------------
-- 5. Show missing hubs from your list
--------------------------------------
SELECT DISTINCT
    'Not Found in DB' AS Status,
    FullOld AS MissingValue
FROM #CheckExisting
WHERE ExistingName IS NULL;



/* --------------------------------------------------------
   6. CLEAN DUPLICATES CREATED BY THE UPDATE
   -------------------------------------------------------- */

WITH TargetNames AS (
    SELECT DISTINCT FullNew AS HubName
    FROM #CheckExisting
    WHERE ExistingName IS NOT NULL
),
Dup AS (
    SELECT 
        h.HubId,
        h.HubName,
        ROW_NUMBER() OVER (PARTITION BY h.HubName ORDER BY h.HubId) AS rn
    FROM account.Hub h
    JOIN TargetNames t
        ON LTRIM(RTRIM(h.HubName)) = LTRIM(RTRIM(t.HubName))
)

DELETE FROM account.Hub
WHERE HubId IN (
    SELECT HubId
    FROM Dup
    WHERE rn > 1
);


--------------------------------------
-- 7. Show cleanup result
--------------------------------------
SELECT DISTINCT
    'Duplicate Cleanup Performed' AS Status,
    FullNew AS HubName
FROM #CheckExisting
WHERE ExistingName IS NOT NULL;


DROP TABLE #CheckExisting;
