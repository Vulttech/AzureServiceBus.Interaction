using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Sender
{
    public class AzureServiceBusSender
    {
        private readonly ServiceBusClient _client;

        public AzureServiceBusSender(ServiceBusClient client)
        {
            _client = client;
        }

        public async Task SendMessageAsync<T>(string QueueName, T message)
        {
            var sender = _client.CreateSender(QueueName);
            var json = JsonSerializer.Serialize(message);
            var sbMessage = new ServiceBusMessage(json);
            await sender.SendMessageAsync(sbMessage);
        }
    }
}
