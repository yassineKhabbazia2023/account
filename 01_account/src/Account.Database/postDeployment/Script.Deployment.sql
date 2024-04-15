DECLARE @Counter INT = 1
WHILE @Counter <= 600
BEGIN
    INSERT INTO account.Deployment (AccountId, DeploymentDate, Status)
    VALUES (@Counter, GETDATE(), 1)
    SET @Counter = @Counter + 1
END