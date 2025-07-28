CREATE VIEW [ext].[vw_SignatoryPerAccount] AS
WITH SignatoryRank AS (
    SELECT 
        cnt.FirstName,
        cnt.LastName,
        cnt.Email,
        acc.AccountNumber,
        ROW_NUMBER() OVER (
            PARTITION BY acc.AccountNumber
            ORDER BY cnt.CreationDate ASC
        ) AS RowNum
    FROM [account].[Role] accRole
    JOIN account.Account acc ON acc.AccountId = accRole.AccountId
    JOIN actor.Contact cnt ON cnt.ContactId = accRole.ContactId
    WHERE accRole.IsSignatory = 1
        AND cnt.Type = 'Customer'
        AND cnt.IsActive = 1
)
SELECT 
    FirstName,
    LastName,
    Email,
    AccountNumber
FROM SignatoryRank
WHERE RowNum = 1;