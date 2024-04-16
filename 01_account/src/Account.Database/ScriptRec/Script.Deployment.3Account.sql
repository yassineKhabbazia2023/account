BEGIN
    INSERT INTO account.Deployment (AccountId, DeploymentDate, Status)
    VALUES
    (601, GETDATE(), 1),
    (602, GETDATE(), 1),
    (603, GETDATE(), 1)
END