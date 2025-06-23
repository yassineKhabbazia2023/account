Update [account].[Role] set ActionLevel = 4 where IsCustomerRelation = 1 
and ContactId in (select ContactId from actor.[Contact] c where c.[Type] = 'Collaborator')