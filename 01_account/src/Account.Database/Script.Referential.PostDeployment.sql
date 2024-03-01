/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

:r .\postDeployment\Hub.Refential.sql
:r .\postDeployment\Script.Naf.sql
:r .\postDeployment\Script.Account.sql
:r .\postDeployment\Script.Contact.sql
:r .\postDeployment\Script.Address.sql
:r .\postDeployment\Script.Phone.sql
:r .\postDeployment\Script.Role.sql
:r .\postDeployment\Script.Deployment.sql
:r .\postDeployment\Script.Delegation.sql