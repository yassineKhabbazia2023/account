// <copyright file="UpdateRoleCustomerRelationResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class UpdateRoleCustomerRelationResponse
{
    public List<UpdateRoleCustomerRelationItemResponse> Succeeded { get; set; } = new();

    public List<UpdateRoleCustomerRelationItemResponse> Failed { get; set; } = new();
}
