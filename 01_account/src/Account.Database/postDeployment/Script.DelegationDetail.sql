DECLARE @delegRotschild int, @delegAlstom1 int, @delegAlstom2 int, @delegImagotag1 int, @delegImagotag2 int, @delegFleury int, @delegNexity int;

SET @delegRotschild = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactRothschild);
SET @delegAlstom1 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactAlstom1 AND Note IS NULL);
SET @delegAlstom2 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactAlstom1 AND Note IS NOT NULL);
SET @delegImagotag1 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactImagotag2 AND Status = 'disabled');
SET @delegImagotag2 = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactImagotag2 AND Status = 'pending');
SET @delegFleury = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactFleury);
SET @delegNexity = (SELECT DelegationId FROM account.Delegation WHERE DelegatorId = @contactNexity);

INSERT INTO account.DelegationDetail(DelegationId, AccountId, DelegateeId)
VALUES
(@delegRotschild, @accountRothschild, @contactRenault),
(@delegAlstom1, @accountAsltom, @contactAlstom2),
(@delegAlstom2, @accountAsltom, @contactRenault),
(@delegImagotag1, @accountImagotag, @contactImagotag1),
(@delegImagotag2, @accountImagotag, @contactImagotag1),
(@delegFleury, @accountFleury, @contactAlstom2),
(@delegNexity, @accountNexity, @contactNexity2);