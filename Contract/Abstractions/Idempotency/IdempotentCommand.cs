﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Contract.Abstractions.Idempotency
{
    public abstract record IdempotentCommand
    {
        public Guid RequestId { get; private set; }

        public void SetRequestId(Guid guid)
        {
            RequestId = guid;
        }
    };
}
