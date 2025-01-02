//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Domain.Primitives;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;

//namespace Persistence.Interceptors
//{
//    //public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
//    //{
//    //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
//    //                                                                          InterceptionResult<int> result,
//    //                                                                          CancellationToken cancellationToken = default)
//    //    {
//    //        DbContext? dbContext = eventData.Context;

//    //        if (dbContext is null)
//    //        {
//    //            return base.SavingChangesAsync(eventData, result, cancellationToken);
//    //        }

//    //        dbContext.ChangeTracker
//    //            .Entries<AggregateRoot>()
//    //            .Select(x => x.Entity)
//    //            .SelectMany(x =>
//    //            {

//    //            }).ToList();

//    //        return base.SavingChangesAsync(eventData, result, cancellationToken);
//    //    }
//    //}
//}
