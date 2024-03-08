INSERT INTO account.Deployment (AccountId, DeploymentDate, Status)
VALUES
(@accountRothschild, CAST ('2014-01-22' AS datetime2), 2),
(@accountRenault, CAST ('2001-02-01' AS datetime2), 2),
(@accountAsltom, CAST ('2005-03-01' AS datetime2), 1),
(@accountSrp, CAST ('2010-07-27' AS datetime2), 0),
(@accountImagotag, CAST ('2017-10-01' AS datetime2), 0),
(@accountFleury, CAST ('1957-01-01' AS datetime2), 2),
(@accountChaussLouis, CAST ('2003-04-24' AS datetime2), 2),
(@accountChaussLouis, CAST ('2005-01-18' AS datetime2), 1),
(@accountAbricotine, CAST ('2012-01-01' AS datetime2), 2),
(@accountMogador, CAST ('2023-10-26' AS datetime2), 0),
(@accountNexity, CAST ('2002-11-21' AS datetime2), 2);