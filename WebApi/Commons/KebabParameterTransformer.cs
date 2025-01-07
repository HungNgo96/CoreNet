// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace WebApi.Commons
{
    /// <summary>
    ///
    /// </summary>
    public class KebabParameterTransformer : IOutboundParameterTransformer
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string? TransformOutbound(object? value)
        {
            if (value == null)
            {
                return null;
            }

            string text = value.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                return Regex.Replace(text, "([a-z])([A-Z])", "$1-$2").ToLower();
            }

            return null;
        }
    }
}
