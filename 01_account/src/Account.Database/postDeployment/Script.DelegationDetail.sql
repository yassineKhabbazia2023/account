DECLARE @delegRotschild int, @delegAlstom1 int, @delegAlstom2 int, @delegImagotag1 int, @delegImagotag2 int, @delegFleury int, @delegNexity int;

SET @delegRotschild = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactRothschild);
SET @delegAlstom1 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactAlstom1 AND Note IS NULL);
SET @delegAlstom2 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactAlstom1 AND Note IS NOT NULL);
SET @delegImagotag1 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactImagotag2 AND Status = 'disabled');
SET @delegImagotag2 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactImagotag2 AND Status = 'pending');
SET @delegFleury = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactFleury);
SET @delegNexity = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactNexity);

INSERT INTO account.DelegationDetail(DelegationId, AccountId)
VALUES
(@delegRotschild, @accountRothschild),
(@delegAlstom1, @accountAsltom),
(@delegAlstom2, @accountAsltom),
(@delegImagotag1, @accountImagotag),
(@delegImagotag2, @accountImagotag),
(@delegFleury, @accountFleury),
(@delegNexity, @accountNexity);