// <copyright file="Errors.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

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
    }
}
