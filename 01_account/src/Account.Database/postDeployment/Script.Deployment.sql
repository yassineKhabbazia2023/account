BEGIN
    INSERT INTO account.Deployment (AccountId, DeploymentDate, Status)
    SELECT Number, GETDATE(), 1
        FROM (
        SELECT TOP 600 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Number
        FROM sys.columns AS c1
        CROSS JOIN sys.columns AS c2
    ) AS Numbers;
END