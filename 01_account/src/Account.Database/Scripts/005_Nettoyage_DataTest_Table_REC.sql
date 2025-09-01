DECLARE @SearchTerm NVARCHAR(100) = N'Test Hermes MEG'; -- remplacer par le mail pour un contact
DECLARE @Schema SYSNAME = N'account'; -- ex: actor, account, ..
DECLARE @Table SYSNAME = N'Account'; -- ex: Contact, Company, ...
DECLARE @BaseIdColumn SYSNAME = N'AccountId'; -- ex: CompanyId, ContactId, ..
DECLARE @BaseSearchColumn SYSNAME = N'LegalName'; -- ex: AccountLegalName, Email, ...

IF OBJECT_ID('tempdb..#IdsToDelete') IS NOT NULL DROP TABLE #IdsToDelete;
CREATE TABLE #IdsToDelete (Id INT);  

DECLARE @sql NVARCHAR(MAX);

SET @sql = N'
    INSERT INTO #IdsToDelete(Id)
    SELECT ' + QUOTENAME(@BaseIdColumn) + '
    FROM ' + + QUOTENAME(@Schema) + '.' + QUOTENAME(@Table) + '
    WHERE ' + QUOTENAME(@BaseSearchColumn) + ' LIKE ''%' + @SearchTerm + N'%'';';

PRINT @sql;
EXEC(@sql);

DECLARE @deleteChildren NVARCHAR(MAX) = N'';

SELECT @deleteChildren = STRING_AGG(
    'DELETE c FROM ' + QUOTENAME(sch.name) + '.' + QUOTENAME(fk_tab.name) + ' c
     WHERE c.' + QUOTENAME(fk_col.name) + ' IN (SELECT Id FROM #IdsToDelete);'
, CHAR(13) + CHAR(10))
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables fk_tab ON fkc.parent_object_id = fk_tab.object_id
JOIN sys.schemas sch ON fk_tab.schema_id = sch.schema_id
JOIN sys.columns fk_col ON fkc.parent_object_id = fk_col.object_id AND fkc.parent_column_id = fk_col.column_id
JOIN sys.tables pk_tab ON fkc.referenced_object_id = pk_tab.object_id
JOIN sys.schemas pk_sch ON pk_tab.schema_id = pk_sch.schema_id
WHERE pk_tab.name = @Table
  AND pk_sch.name = @Schema;


PRINT @deleteChildren;
EXEC(@deleteChildren);

DECLARE @deleteMainTable NVARCHAR(MAX);

SET @deleteMainTable = N'DELETE FROM ' + QUOTENAME(@Schema) + '.' + QUOTENAME(@Table) + '
            WHERE ' + QUOTENAME(@BaseIdColumn) + ' IN (SELECT Id FROM #IdsToDelete);';

PRINT @deleteMainTable;
EXEC(@deleteMainTable);
