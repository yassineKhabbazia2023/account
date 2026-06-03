-- =============================================================================
-- Script  : 007_Update_Label_AM_CLP.sql
-- Objet   : Correction des libellés (CustomerLabel / CollaboratorLabel)
--           pour les codes 'AM' et 'CLP' dans la table [account].[Label].
-- Cible   : Base Account
-- =============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Update du code 'AM'
    UPDATE [account].[Label]
    SET
        CustomerLabel     = N'Maître de Dossier',
        CollaboratorLabel = N'Maître de Dossier',
        Description       = N'Maître de Dossier'
    WHERE Code = 'AM';

    PRINT CONCAT('Code AM  - lignes mises à jour : ', @@ROWCOUNT);

    -- Update du code 'CLP'
    UPDATE [account].[Label]
    SET
        CustomerLabel     = N'Responsable de Compte',
        CollaboratorLabel = N'Responsable de Compte',
        Description       = N'Responsable de Compte'
    WHERE Code = 'CLP';

    PRINT CONCAT('Code CLP - lignes mises à jour : ', @@ROWCOUNT);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
      ROLLBACK TRANSACTION;
END CATCH;
