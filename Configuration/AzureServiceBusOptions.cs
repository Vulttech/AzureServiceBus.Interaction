using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Configuration
{
    public class AzureServiceBusOptions
    {
        public string ConnectionString { get; set; } = string.Empty;

        internal List<AzureServiceBusSubscription> Subscriptions { get; } = new();

        public void RegisterQueue<T>(string queueName, Func<T, Task> handler)
        {
            Subscriptions.Add(new AzureServiceBusSubscription
            {
                QueueName = queueName,
                Handler = async (msg) =>
                {
                    if (msg is T typed)
                        await handler(typed);
                },
                HandlerType = typeof(T)
            });
        }
    }

    internal class AzureServiceBusSubscription
    {
        public string QueueName { get; set; } = string.Empty;
        public Type HandlerType { get; set; } = null!;
        public Func<object, Task> Handler { get; set; } = null!;
    }
}
