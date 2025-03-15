using Domain.Core.SharedKernel;

namespace Infrastructure.MessageBroker.RabbitMQ
{
    public class RabbitMQOptions : IAppOptions
    {
        static string IAppOptions.ConfigSectionPath => nameof(RabbitMQOptions);
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public ExchangeOptions Exchange { get; set; } = new();
        public QueueOptions Queue { get; set; } = new();
        public RetryPolicyOptions RetryPolicy { get; set; } = new();
    }

    public class ExchangeOptions
    {
        public string Name { get; set; } = "my_exchange";
        public string Type { get; set; } = "direct";
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
    }

    public class QueueOptions
    {
        public string Name { get; set; } = "my_queue";
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
    }

    public class RetryPolicyOptions
    {
        public int MaxRetryAttempts { get; set; } = 5;
        public int InitialInterval { get; set; } = 1000; // milliseconds
        public int MaxInterval { get; set; } = 5000; // milliseconds
        public double Multiplier { get; set; } = 2.0;
    }
}
