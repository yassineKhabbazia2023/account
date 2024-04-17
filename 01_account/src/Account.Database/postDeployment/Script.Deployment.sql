-- LOG
-- US 315015: Add 3 account
BEGIN
    INSERT INTO account.Deployment (AccountId, DeploymentDate, Status)
    SELECT Number, GETDATE(), 1
        FROM (
        SELECT TOP 603 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Number
        FROM sys.columns AS c1
        CROSS JOIN sys.columns AS c2
    ) AS Numbers;
END