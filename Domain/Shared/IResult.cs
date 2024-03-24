// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain.Shared
{
    public interface IResult<out T> : IResult
    {
        T? Data { get; }
    }

    public interface IResult
    {
        string? Message { get; set; }
        bool Succeeded { get; set; }
        int Code { get; set; }
    }
}
