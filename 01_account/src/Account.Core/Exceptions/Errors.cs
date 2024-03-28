// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Exceptions
{
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
    }
}
