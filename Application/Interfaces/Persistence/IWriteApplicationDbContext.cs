// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces.Persistence
{
    public interface IWriteApplicationDbContext : IApplicationDbContext
    {
        public DatabaseFacade Database { get; }
    }
}
