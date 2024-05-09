// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core
{
    internal class Cqrs
    {
       // public interface IListQuery<TResponse> : IQuery<TResponse>
       //where TResponse : notnull
       // {
       //     public List<string> Includes { get; init; }
       //     public List<FilterModel> Filters { get; init; }
       //     public List<string> Sorts { get; init; }
       //     public int Page { get; init; }
       //     public int PageSize { get; init; }
       // }

        //public interface IItemQuery<TId, TResponse> : IQuery<TResponse>
        //    where TId : struct
        //    where TResponse : notnull
        //{
        //    public List<string> Includes { get; init; }
        //    public TId Id { get; init; }
        //}
    }
}
