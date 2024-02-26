// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Exceptions
{
    public static class Errors
    {
        public static readonly string NotFoundAccountCode = "ACC001";
        public static readonly string NotFoundAccountMessage = "L'identifiant de l'entité indiquée est incorrect";

        public static readonly string NotFoundContactCode = "ACC002";
        public static readonly string NotFoundContactMessage = "Le contact avec l'identifiant {0} est introuvable";

        public static readonly string CreateDelegationMessage = "ACC003";
        public static readonly string CreateDelegationCode = "Impossible de créer une délégation : les informations fournies dans la requête sont incorrectes.";

        public static readonly string NotFoundRoleCode = "ACC004";
        public static readonly string NotFoundRoleMessage = "Le contact avec l'identifiant {0} n'a aucun role sur l'account {1}";

        public static readonly string DelegationEndDateInvalidCode = "ACC005";
        public static readonly string DelegationEndDateInvalidMessage = "Impossible de créer une délégation : La date de début de la délégation ne peut pas être supérieur à la date de fin.";

        public static readonly string DelegationStartDateInvalidCode = "ACC006";
        public static readonly string DelegationStartDateInvalidMessage = "Impossible de créer une délégation : La date de début de la délégation ne peut pas être null.";
    }
}
