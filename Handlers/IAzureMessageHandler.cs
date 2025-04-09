using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus.Interaction.Handlers
{
    public interface IAzureMessageHandler<T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
