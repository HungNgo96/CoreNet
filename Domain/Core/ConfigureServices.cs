// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Core.SharedKernel.Correlation;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Core
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddCorrelationGenerator(this IServiceCollection services) =>
             services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
    }
}
