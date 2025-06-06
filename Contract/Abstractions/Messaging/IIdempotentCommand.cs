﻿namespace Contract.Abstractions.Messaging
{
    public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
    {
        Guid RequestId { get; set; }
    }

    //public interface IIdempotentCommand : ICommand
    //{
    //    Guid RequestId { get; init; }
    //}
}
