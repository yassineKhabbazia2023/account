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
		WHEN c.Status IS NOT NULL THEN c.Status
		ELSE 'Collaborator'
	END AS Status,
	count(c.Email) AS Nombre
  FROM [actor].[Contact] c
  INNER JOIN [account].[Role] r ON r.ContactId = c.ContactId
  WHERE r.AccountId NOT IN (SELECT AccountId FROM [account].[Account] WHERE AccountType = 'TEST' OR AccountType IS NULL OR SourceName = 'Entity Creator')
  GROUP BY c.Status