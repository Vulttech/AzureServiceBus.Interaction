using Azure.Messaging.ServiceBus;
using AzureServiceBus.Interaction.Configuration;
using AzureServiceBus.Interaction.Processor;
using AzureServiceBus.Interaction.Sender;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBus.Interaction;

public static class AzureServiceBusServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBusInteraction(this IServiceCollection services, Action<AzureServiceBusOptions> configure)
    {
        var options = new AzureServiceBusOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton(new ServiceBusClient(options.ConnectionString));
        services.AddSingleton<IAzureServiceBusSender, AzureServiceBusSender>();
        services.AddHostedService<AzureServiceBusBackgroundProcessor>();

        return services;
    }
}
