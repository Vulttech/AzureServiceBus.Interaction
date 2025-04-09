using Azure.Messaging.ServiceBus;
using AzureServiceBus.Interaction.Handlers;
using AzureServiceBus.Interaction.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Extensions
{
    public static class AzureServiceBusExtensions
    {
        public static IServiceCollection AddAzureServiceBusConsumer<TMessage, THandler>(
            this IServiceCollection services,
            string queueName)
            where THandler : class, IAzureMessageHandler<TMessage>
        {
            services.AddSingleton<IAzureMessageHandler<TMessage>, THandler>();

            services.AddHostedService(provider =>
            {
                var client = provider.GetRequiredService<ServiceBusClient>();
                var handler = provider.GetRequiredService<IAzureMessageHandler<TMessage>>();
                var logger = provider.GetRequiredService<ILogger<AzureQueueBackgroundProcessor<TMessage>>>();

                return new AzureQueueBackgroundProcessor<TMessage>(client, handler, logger, queueName);
            });

            return services;
        }
    }
}
