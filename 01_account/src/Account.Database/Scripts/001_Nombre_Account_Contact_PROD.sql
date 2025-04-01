-- ACCOUNT
SELECT
    CASE 
		WHEN a.IsActive = 0 THEN 'Revoked'
        WHEN d.Status = 1 THEN 'ToDeploy'  
        WHEN d.Status = 2 THEN 'InProgress' 
        WHEN d.Status = 3 THEN 'Connected'  
        ELSE 'Revoked'
    END AS Status, 
	count(a.AccountNumber) AS Nombre 
  FROM [account].[Account] a
  INNER JOIN [account].[Deployment] d ON d.AccountId = a.AccountId
  WHERE AccountType <> 'TEST' AND AccountType IS NOT NULL AND SourceName <> 'Entity Creator'
  GROUP BY d.Status, a.IsActive


-- CONTACT
  SELECT 
    CASE
      WHEN c.IsActive = 0 THEN 'Revoked'
      WHEN c.Status IS NOT NULL THEN c.Status
      ELSE 'Unknown'
    END AS Status,
    count(DISTINCT c.ContactId) AS Nombre
  FROM [actor].[Contact] c
  WHERE c.Type = 'Customer' 
	  AND c.ContactId NOT IN (SELECT 
                            DISTINCT r.ContactId 
                          FROM [account].[Role] r
                          INNER JOIN [account].[Account] a ON a.AccountId = r.AccountId 
                          WHERE AccountType = 'Test' OR AccountType IS NULL OR SourceName = 'Entity Creator')
  GROUP BY c.Status, c.Type, c.IsActive