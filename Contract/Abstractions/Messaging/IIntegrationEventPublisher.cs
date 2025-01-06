// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Contract.Abstractions.Messaging
{
    public interface IIntegrationEventPublisher
    {
        /// <summary>
        /// Publishes the specified integration event to the message queue.
        /// </summary>
        /// <param name="integrationEvent">The integration event.</param>
        /// <returns>The completed task.</returns>
        void Publish(IIntegrationEvent integrationEvent);
    }
}
