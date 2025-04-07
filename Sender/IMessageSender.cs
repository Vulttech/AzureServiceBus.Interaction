using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Sender
{
    public interface IMessageSender
    {
        Task SendMessageAsync<T>(string QueueName, T message);
    }
}
