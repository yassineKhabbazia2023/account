// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Exceptions;

public static class Errors
{
    public static readonly string NotFoundAccountCode = "ACC001";
    public static readonly string NotFoundAccountMessage = "L'entité avec l'identifiant {0} est introuvable";

    public static readonly string NotFoundContactCode = "ACC002";
    public static readonly string NotFoundContactMessage = "Le contact avec l'identifiant {0} est introuvable";

    public static readonly string CreateDelegationCode = "ACC003";
    public static readonly string CreateDelegationMessage = "Impossible de créer une délégation : les informations fournies dans la requête sont incorrectes.";

    public static readonly string NotFoundRoleCode = "ACC004";
    public static readonly string NotFoundRoleMessage = "Le contact avec l'identifiant {0} n'a aucun role sur l'account {1}";

    public static readonly string BadRequestDeleteDelegationCode = "ACC005";
    public static readonly string BadRequestDeleteDelegationMessage = "Impossible de supprimer la délégation : les informations fournies dans la requête sont incorrectes.";

    public static readonly string NotFoundDelegationCode = "ACC006";
    public static readonly string NotFoundDelegationMessage = "La délégation avec l'identifiant {0} est introuvable";

    public static readonly string DelegationEndDateInvalidCode = "ACC007";
    public static readonly string DelegationEndDateInvalidMessage = "Impossible de créer une délégation : La date de début de la délégation ne peut pas être supérieur à la date de fin.";

    public static readonly string DelegationStartDateInvalidCode = "ACC008";
    public static readonly string DelegationStartDateInvalidMessage = "Impossible de créer une délégation : La date de début de la délégation ne peut pas être null.";

    public static readonly string BadRequestRoleCode = "ACC009";
    public static readonly string BadRequestRoleMessage = "Impossible de créer un role : les informations fournies dans la requête sont incorrectes.";

    public static readonly string BadRequestAccountPatchCode = "ACC010";
    public static readonly string BadRequestAccountPatchMessage = "Impossible de mettre à jour l'entité morale : les informations fournies dans la requête sont incorrectes.";

    public static readonly string CannotDeleteSignatoryCode = "ACC011";
    public static readonly string CannotDeleteSignatoryMessage = "Impossibe de supprimer le role: C'est le seul signataire.";

    public static readonly string NotFoundAccountsCode = "ACC012";
    public static readonly string NotFoundAccountsMessage = "Une des entités est introuvable";

    public static readonly string NotFoundContactsCode = "ACC013";
    public static readonly string NotFoundContactsMessage = "Un des contacts est introuvable";

    public static readonly string NotFoundRoleContactCode = "ACC014";
    public static readonly string NotFoundRoleContactMessage = "Le contact avec l'identifiant {0} n'a aucun role";

    public static readonly string NotFoundTopicName = "ACC015";
    public static readonly string NotFoundTopicNameMessage = "Le nom du topic doit être renseigné";

    public static readonly string NotFoundServiceBusNamespaceCode = "ACC016";
    public static readonly string NotFoundServiceBusNamespaceMessage = "La namespace du service bus doit être renseignée";

    public static readonly string BadRequestDeploymentStatusCode = "ACC017";
    public static readonly string BadRequestDeploymentStatusMessage = "La statut de déploiement {0} n'existe pas";

    public static readonly string DontHaveRightAccountsCode = "ACC018";
    public static readonly string DontHaveRightAccountsMessage = "Impossible de créer une délégation: Vous avez pas le droit sur un des accounts.";

    public static readonly string BadRequestContactsAccountCode = "ACC019";
    public static readonly string BadRequestContactsAccountMessage = "Le champ demandé {0} n'existe pas";

    public static readonly string NotFoundManagedIdentityClientIdCode = "ACC020";
    public static readonly string NotFoundManagedIdentityClientIdMessage = "L'identité du clientID doit être renseignée";

    public static readonly string BadRequestExistingRoleCode = "ACC021";
    public static readonly string BadRequestExistingRoleMessage = "Le role ContactId {0}/AccountId {1} existe déjà";

    public static readonly string BadRequestAccountIdAndAccountNumberNullCode = "ACC022";
    public static readonly string BadRequestAccountIdAndAccountNumberNullMessage = "Veuillez fournir au moins l'AccountId ou l'AccountNumber";

    public static readonly string BadRequestClientCannotDelegateCode = "ACC023";
    public static readonly string BadRequestClientCannotDelegateMessage = "Un client ne peut pas émettre ou recevoir de délégation";

    public static readonly string NotFoundDatabaseConnectionStringCode = "ACC024";
    public static readonly string NotFoundDataBaseConnectionStringMessage = "Connection String au base de donnée est null ou vide!";

    public static readonly string NotFoundApplicationInsightConnectionStringCode = "ACC025";
    public static readonly string NotFoundApplicationInsightConnectionStringMessage = "Connection string du application insight est null ou vide";

    public static readonly string RoleNotFoundCode = "ACC026";
    public static readonly string RoleNotFoundMessage = "Le role accountId {0} et contactId {1} est introuvable";

    public static readonly string NullDelegationRequestCode = "ACC027";
    public static readonly string NullDelegationRequestMessage = "le Delegation request est null";

    public static readonly string NullArgumentCode = "ACC028";
    public static readonly string NullArgumentMessage = "Argument {0} est null ou vide!";

    public static readonly string CurrentUserWasNotFoundInHeadersCode = "ACC029";
    public static readonly string CurrentUserWasNotFoundInHeadersMessage = "Le CurrentUser n'a pas été transmis via header.";

    public static readonly string InvalidCurrentUserFormatCode = "ACC030";
    public static readonly string InvalidCurrentUserFormatMessage = "Le header CurrentUser doit être un entier valide.";

    public static readonly string ContactIdAndEmailNullCode = "ACC031";
    public static readonly string ContactIdAndEmailNullMessage = "Veuillez fournir au moins le ContactId ou l'email.";

    public static readonly string CodeAlreadyExistsCode = "ACC032";
    public static readonly string CodeAlreadyExitsMessage = "Le code {0} existe déjà dans la base.";

    public static readonly string RoleLabelAlreadyExistsCode = "ACC033";
    public static readonly string RoleLabelAlreadyExistsMessage = "Ce contact a déjà ce libellé.";

    public static readonly string LabelNotFoundCode = "ACC034";
    public static readonly string LabelNotFoundMessage = "Aucun libellé trouvé.";

    public static readonly string NoClientLabelCode = "ACC035";
    public static readonly string NoClientLabelMessage = "Impossible d'ajouter des libellés pour un client.";

    public static readonly string InvalidActionLevelCode = "ACC036";
    public static readonly string InvalidActionLevelMessage = "Le niveau d'action ne peut pas être inférieur à 0.";

    public static readonly string BadRequestOfferEligibilityPatchCode = "ACC037";
    public static readonly string BadRequestOfferEligibilityPatchMessage = "Impossible de mettre à jour l'éligibilité de l'offre : les informations fournies dans la requête sont incorrectes.";

    public static readonly string NotFoundOfferEligibilityCode = "ACC038";
    public static readonly string NotFoundOfferEligibilityMessage = "L'éligibilité de l'offre avec l'identifiant {0} est introuvable";

    public static readonly string AlreadyActiveOfferEligibilityCode = "ACC039";
    public static readonly string AlreadyActiveOfferEligibilityMessage = "L'offre est déjà active";

    public static readonly string NotPermittedActionCode = "ACC040";
    public static readonly string NotPermittedActionMessage = "L'utilisateur ne peut pas effectuer cette action";

    public static readonly string InvalidAccountFieldsCode = "ACC041";
    public static readonly string InvalidAccountFieldsMessage = "Les champs obligatoires sont manquants ou invalides. Veuillez compléter les informations nécessaires.";

    public static readonly string SelfDelegationRequestCode = "ACC042";
    public static readonly string SelfDelegationRequestMessage = "Impossible de créer une demande de délégation : vous ne pouvez pas vous demander une délégation à vous-même.";

    public static readonly string RequesterAlreadyHasAccessCode = "ACC043";
    public static readonly string RequesterAlreadyHasAccessMessage = "Impossible de créer une demande de délégation : vous avez déjà accès à ce dossier.";

    public static readonly string RecipientDoesNotHaveAccessCode = "ACC044";
    public static readonly string RecipientDoesNotHaveAccessMessage = "Impossible de créer une demande de délégation : le destinataire avec l'identifiant {0} n'a pas accès à ce dossier.";

    public static readonly string DelegationRequestAlreadyPendingCode = "ACC045";
    public static readonly string DelegationRequestAlreadyPendingMessage = "Impossible de créer une demande de délégation : une demande en attente existe déjà pour ce dossier.";

    public static readonly string DelegationRequestNotFoundCode = "ACC046";
    public static readonly string DelegationRequestNotFoundMessage = "La demande de délégation avec l'identifiant {0} est introuvable.";

    public static readonly string DelegationRequestNotPendingCode = "ACC047";
    public static readonly string DelegationRequestNotPendingMessage = "La demande de délégation n'est plus en attente de traitement.";

    public static readonly string DelegationRequestNotRecipientCode = "ACC048";
    public static readonly string DelegationRequestNotRecipientMessage = "Vous n'êtes pas destinataire de cette demande de délégation.";

    public static readonly string DelegationRequestIdsEmptyCode = "ACC049";
    public static readonly string DelegationRequestIdsEmptyMessage = "Veuillez fournir au moins un identifiant de demande de délégation.";

    public static readonly string DelegationRequestAllInvalidCode = "ACC050";
    public static readonly string DelegationRequestAllInvalidMessage = "Aucune demande de délégation valide n'a pu être traitée.";
    public static readonly string CannotDeleteSignatoryProspectCode = "ACC051";
    public static readonly string CannotDeleteSignatoryProspectMessage = "Impossible de supprimer le rôle : la suppression d'un signataire n'est pas autorisée pour un compte de type prospect.";

    public static readonly string CannotDeleteExclusiveLabelProspectCode = "ACC052";
    public static readonly string CannotDeleteExclusiveLabelProspectMessage = "Impossible de supprimer le libellé : la suppression d'un libellé AM ou CLP n'est pas autorisée pour un compte de type prospect.";
}
