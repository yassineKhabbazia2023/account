// <copyright file="Errors.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace Pulse.Account.Core.Exceptions
{
    public static class Errors
    {
        public static readonly string NotFoundAccountCode = "X001";
        public static readonly string NotFoundAccountMessage = "L'identifiant de l'entité indiquée est incorrect";

        public static readonly string NotFoundContactCode = "X002";
        public static readonly string NotFoundContactMessage = "Le contact avec l'identifiant {0} est introuvable";

        public static readonly string CreateDelegationMessage = "X003";
        public static readonly string CreateDelegationCode = "Impossible de créer une délégation : les informations fournies dans la requête sont incorrectes.";
    }
}
