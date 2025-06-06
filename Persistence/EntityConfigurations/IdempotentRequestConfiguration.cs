﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Idempotency;

namespace Persistence.EntityConfigurations
{
    public sealed class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequest>
    {
        public void Configure(EntityTypeBuilder<IdempotentRequest> builder)
        {
            builder.ToTable(nameof(IdempotentRequest) + "s");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
        }
    }
}
