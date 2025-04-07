using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Processor
{
        public class AzureQueueBackgroundProcessor : BackgroundService, IMessageProcessor
        {
            private readonly ServiceBusClient _client;
            private readonly ILogger<AzureQueueBackgroundProcessor> _logger;

            private string? _queueName;
            private Func<object, Task>? _handler;
            private Type? _handlerType;
            private ServiceBusProcessor? _processor;

            public AzureQueueBackgroundProcessor(ServiceBusClient client, ILogger<AzureQueueBackgroundProcessor> logger)
            {
                _client = client;
                _logger = logger;
            }

            public async Task ProcessAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default)
            {
                _queueName = queueName;
                _handlerType = typeof(T);
                _handler = async (obj) =>
                {
                    if (obj is T typed)
                        await handler(typed);
                };

                _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

                _processor.ProcessMessageAsync += ProcessMessageAsync;
                _processor.ProcessErrorAsync += ErrorHandlerAsync;

                await _processor.StartProcessingAsync(cancellationToken);
            }

            private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
            {
                try
                {
                    if (_handlerType == null || _handler == null)
                        return;

                    var body = args.Message.Body.ToString();
                    var deserialized = JsonSerializer.Deserialize(body, _handlerType!);

                    if (deserialized != null)
                        await _handler(deserialized);

                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje");
                    await args.AbandonMessageAsync(args.Message); // reintentos automáticos si están configurados en la cola
                }
            }

            private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
            {
                _logger.LogError(args.Exception, "Error en Azure Service Bus: {ErrorSource}", args.ErrorSource);
                return Task.CompletedTask;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                // Esperamos a que ProcessAsync sea llamado desde otro servicio
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }

            public override async Task StopAsync(CancellationToken cancellationToken)
            {
                if (_processor != null)
                    await _processor.CloseAsync(cancellationToken);

                await base.StopAsync(cancellationToken);
            }
        }
    
}
