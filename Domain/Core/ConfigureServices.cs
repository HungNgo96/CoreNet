// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.AppSettings;
using Domain.Core.SharedKernel;
using Domain.Core.SharedKernel.Correlation;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Core
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddCorrelationGenerator(this IServiceCollection services)
        {
            NumericIdGenerator.Init();
            return services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
        }


        /// <summary>
        /// Adds options with validation to the service collection.
        /// </summary>
        /// <typeparam name="TOptions">The type of options to add.</typeparam>
        /// <param name="services">The service collection.</param>
        public static IServiceCollection AddOptionsWithValidation<TOptions>(this IServiceCollection services)
            where TOptions : class, IAppOptions
        {
            return services
                .AddOptions<TOptions>()
                .BindConfiguration(TOptions.ConfigSectionPath, binderOptions => binderOptions.BindNonPublicProperties = true)
                .ValidateDataAnnotations()
                .ValidateOnStart()
                .Services;
        }
    }
}
