// <copyright file="EntityContextExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace Pulse.Authorization.Infrastructure.Extensions
{
    public static class EntityContextExtensions
    {
        public static void HandleEFCoreFailure<T>(this T context) where T : DbContext
        {
            context.SaveChangesFailed += (s, e) =>
            {
                context.ChangeTracker.Clear();
            };
        }
    }
}
