using Azure.Messaging.ServiceBus;
using Polly;
using System.Text.Json;

namespace AzureServiceBus.Interaction.Sender;

public interface IAzureServiceBusSender
{
    Task SendMessageAsync<T>(string queueName, T message);
}

public class AzureServiceBusSender : IAzureServiceBusSender
{
    private readonly ServiceBusClient _client;

    public AzureServiceBusSender(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task SendMessageAsync<T>(string queueName, T message)
    {
        var sender = _client.CreateSender(queueName);
        var json = JsonSerializer.Serialize(message);
        var sbMessage = new ServiceBusMessage(json);

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i)); // configurable a futuro

        await retryPolicy.ExecuteAsync(() => sender.SendMessageAsync(sbMessage));
    }
}
