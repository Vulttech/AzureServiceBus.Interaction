# AzureServiceBus.Interaction

Librería liviana para facilitar la **integración con Azure Service Bus (colas)** en proyectos .NET. Permite enviar y recibir mensajes JSON de forma simple, soporta múltiples consumidores simultáneos, reintentos automáticos y se registra fácilmente con `IServiceCollection`.

---

## 🚀 Características principales

- ✅ Envío de mensajes JSON serializados
- ✅ Consumo automático con `BackgroundService`
- ✅ Registro de múltiples colas simultáneas
- ✅ Reintentos automáticos con Polly
- ✅ Configuración por código (no depende de appsettings.json)
- ✅ Pensado para microservicios backend
- 🔒 Dead-letter handling básico integrado
- 🧪 Preparado para convertirse en paquete NuGet

---

## 📦 Instalación

Agregá la referencia a tu solución:

bash
dotnet add package Polly


🛠️ Registro en Program.cs

using AzureServiceBus.Interaction;
using AzureServiceBus.Interaction.Processor;
using AzureServiceBus.Interaction.Sender;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Leer connection string (podés usar IConfiguration o directamente del código)
var connectionString = builder.Configuration.GetConnectionString("AzureServiceBus");

builder.Services.AddAzureServiceBusInteraction(connectionString);

var app = builder.Build();

app.Run();



📩 Enviar mensajes


public class ClienteCreado
{
    public string Nombre { get; set; }
    public string Email { get; set; }
}


// Dentro de un controller, service, etc.
private readonly IAzureServiceBusSender _sender;

public async Task CrearCliente()
{
    var cliente = new ClienteCreado { Nombre = "Juan", Email = "juan@mail.com" };
    await _sender.SendMessageAsync("clientes-queue", cliente);
}


📥 Consumir mensajes


public class ClienteCreadoHandler : IAzureMessageHandler<ClienteCreado>
{
    public Task HandleAsync(ClienteCreado message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Nuevo cliente: {message.Nombre}");
        return Task.CompletedTask;
    }
}


Registro de handlers

Agregá tu handler como servicio:

builder.Services.AddScoped<IAzureMessageHandler<ClienteCreado>, ClienteCreadoHandler>();


Y registrá la cola a escuchar:

builder.Services.AddAzureServiceBusConsumer<ClienteCreado>("clientes-queue");




⚙️ Estructura
IAzureServiceBusSender: servicio para enviar mensajes

AzureServiceBusBackgroundProcessor: BackgroundService que escucha una o más colas

IAzureMessageHandler<T>: interfaz para implementar handlers de mensajes

Reintentos configurados con Polly (x3 intentos con backoff exponencial)


🧭 Roadmap (próximas mejoras)
Soporte para encabezados (ApplicationProperties)

Programación de mensajes (Scheduled)

Soporte opcional de topics/subscriptions

Publicación del paquete en NuGet.org


🧪 Testing
La librería está diseñada para ser usada en microservicios backend con inyección de dependencias. Se recomienda usarla en proyectos como:

Worker Services

ASP.NET Core APIs

Microservicios desacoplados vía mensajería