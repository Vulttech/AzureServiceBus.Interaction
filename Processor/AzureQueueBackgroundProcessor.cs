using Azure.Messaging.ServiceBus;
using AzureServiceBus.Interaction.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System.Text.Json;

namespace AzureServiceBus.Interaction.Processor;

public class AzureQueueBackgroundProcessor<TMessage> : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IAzureMessageHandler<TMessage> _handler;
    private readonly ILogger<AzureQueueBackgroundProcessor<TMessage>> _logger;
    private readonly string _queueName;

    public AzureQueueBackgroundProcessor(
        ServiceBusClient client,
        IAzureMessageHandler<TMessage> handler,
        ILogger<AzureQueueBackgroundProcessor<TMessage>> logger,
        string queueName)
    {
        _client = client;
        _handler = handler;
        _logger = logger;
        _queueName = queueName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var json = args.Message.Body.ToString();
                var message = JsonSerializer.Deserialize<TMessage>(json);
                if (message != null)
                {
                    var retryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i));

                    await retryPolicy.ExecuteAsync(() => _handler.HandleAsync(message, stoppingToken));
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje en la cola {QueueName}", _queueName);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error en cola {QueueName}: {ErrorSource}", _queueName, args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }
}
