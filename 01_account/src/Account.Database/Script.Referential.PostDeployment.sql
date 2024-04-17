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
:r .\postDeployment\Script.600Accounts.sql
:r .\postDeployment\Script.600Address.sql
:r .\postDeployment\Script.Contact.Pulse.sql
:r .\postDeployment\Script.Contact.Client.sql
:r .\postDeployment\Script.Role.Pulse.sql
:r .\postDeployment\Script.Role.Client.sql
:r .\postDeployment\Script.Role.3Accounts.sql
:r .\postDeployment\Script.Phone.sql
:r .\postDeployment\Script.Deployment.sql