using Azure.Messaging.ServiceBus;
using AzureServiceBus.Interaction.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System.Text.Json;

namespace AzureServiceBus.Interaction.Processor;

public class AzureServiceBusBackgroundProcessor : BackgroundService
{
    private readonly ILogger<AzureServiceBusBackgroundProcessor> _logger;
    private readonly AzureServiceBusOptions _options;
    private readonly ServiceBusClient _client;
    private readonly List<ServiceBusProcessor> _processors = new();

    public AzureServiceBusBackgroundProcessor(
        ILogger<AzureServiceBusBackgroundProcessor> logger,
        AzureServiceBusOptions options,
        ServiceBusClient client)
    {
        _logger = logger;
        _options = options;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var sub in _options.Subscriptions)
        {
            var processor = _client.CreateProcessor(sub.QueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
            });

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var json = args.Message.Body.ToString();
                    var message = JsonSerializer.Deserialize(json, sub.HandlerType);
                    if (message != null)
                    {
                        var retryPolicy = Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i));

                        await retryPolicy.ExecuteAsync(() => sub.Handler(message));
                    }

                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje de la cola {Queue}", sub.QueueName);
                    await args.AbandonMessageAsync(args.Message);
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error en cola {Queue}: {ErrorSource}", sub.QueueName, args.ErrorSource);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
            _processors.Add(processor);
        }

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
            await processor.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
