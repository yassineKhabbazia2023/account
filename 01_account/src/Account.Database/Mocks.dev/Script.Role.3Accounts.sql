-- Role Collab
IF (SELECT COUNT(*) FROM [account].[Role] r JOIN [actor].[Contact] c ON c.ContactId = r.ContactId 
	where r.AccountId in (601, 602, 603) AND c.Type = 'Collaborator') = 0
BEGIN
	DECLARE @Counter INT = 601
	WHILE @Counter <= 603
	BEGIN
		INSERT INTO account.Role(ContactId, AccountId, IsFavorite, IsSignatory, IsDelegation)
		SELECT
			ContactId,
			@Counter,
			0,
			0,
			0
		FROM actor.Contact cnt
		WHERE cnt.Type = 'Collaborator'

		SET @Counter = @Counter + 1
	END
END