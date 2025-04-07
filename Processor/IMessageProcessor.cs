using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Processor
{
    public interface IMessageProcessor
    {
        Task ProcessAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default);
    }
}
