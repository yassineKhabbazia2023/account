------------------- VERIFICATION -------------------
WITH RankedDeployments AS (
    SELECT 
        DeploymentId,
        AccountId,
        DeploymentDate,
        Status,
        ROW_NUMBER() OVER (PARTITION BY AccountId ORDER BY Status DESC, DeploymentDate DESC) AS rn
    FROM 
        account.Deployment
)
SELECT * FROM account.Deployment
WHERE DeploymentId IN (
    SELECT DeploymentId
    FROM RankedDeployments
    WHERE rn > 1
)
ORDER BY AccountId;



------------------- SUPPRESSION -------------------
WITH RankedDeployments AS (
    SELECT 
        DeploymentId,
        AccountId,
        DeploymentDate,
        Status,
        ROW_NUMBER() OVER (PARTITION BY AccountId ORDER BY Status DESC, DeploymentDate DESC) AS rn
    FROM 
        account.Deployment
)
DELETE FROM account.Deployment
WHERE DeploymentId IN (
    SELECT DeploymentId
    FROM RankedDeployments
    WHERE rn > 1
);