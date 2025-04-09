# AzureServiceBus.Interaction

Librería liviana para facilitar la **integración con Azure Service Bus (colas)** en proyectos .NET. Permite enviar y recibir mensajes JSON de forma simple, soporta múltiples consumidores simultáneos, reintentos automáticos con Polly, y se registra fácilmente mediante `IServiceCollection`.

---

## 🚀 Características principales

- ✅ Envío de mensajes JSON serializados
- ✅ Consumo automático con `BackgroundService`
- ✅ Soporte para múltiples colas simultáneas
- ✅ Reintentos automáticos con Polly
- ✅ Configuración por código (sin dependencia de `appsettings.json`)
- ✅ Diseñado exclusivamente para microservicios backend
- 🔒 Dead-letter handling básico integrado
- 🧪 Preparado para publicación como paquete NuGet

---

## 📦 Instalación

### Requisitos

Agregar la referencia a Polly (usado para reintentos):

```bash
dotnet add package Polly
```

Agregá la librería AzureServiceBus.Interaction como proyecto o paquete NuGet (próximamente).

---

## 🛠️ Registro en `Program.cs`

```csharp
using AzureServiceBus.Interaction;
using AzureServiceBus.Interaction.Extensions;
using AzureServiceBus.Interaction.Sender;

var builder = WebApplication.CreateBuilder(args);

// Leer connection string (desde config o directo)
var connectionString = builder.Configuration.GetConnectionString("AzureServiceBus");

builder.Services.AddAzureServiceBusInteraction(options =>
{
    options.ConnectionString = connectionString;
});

// Registrar un consumidor
builder.Services.AddAzureServiceBusConsumer<ClienteCreado, ClienteCreadoHandler>("clientes-queue");

var app = builder.Build();

app.Run();
```

---

## 📩 Enviar mensajes

```csharp
public class ClienteCreado
{
    public string Nombre { get; set; }
    public string Email { get; set; }
}

public class ClienteService
{
    private readonly IAzureServiceBusSender _sender;

    public ClienteService(IAzureServiceBusSender sender)
    {
        _sender = sender;
    }

    public async Task CrearClienteAsync()
    {
        var cliente = new ClienteCreado { Nombre = "Juan", Email = "juan@mail.com" };
        await _sender.SendMessageAsync("clientes-queue", cliente);
    }
}
```

---

## 📥 Consumir mensajes

Implementá la interfaz `IAzureMessageHandler<T>`:

```csharp
public class ClienteCreadoHandler : IAzureMessageHandler<ClienteCreado>
{
    public Task HandleAsync(ClienteCreado message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Nuevo cliente creado: {message.Nombre}");
        return Task.CompletedTask;
    }
}
```

Registrá el handler y la cola en `Program.cs`:

```csharp
builder.Services.AddScoped<IAzureMessageHandler<ClienteCreado>, ClienteCreadoHandler>();
builder.Services.AddAzureServiceBusConsumer<ClienteCreado, ClienteCreadoHandler>("clientes-queue");
```

---

## ⚙️ Estructura

- `IAzureServiceBusSender`: servicio para enviar mensajes
- `IAzureMessageHandler<T>`: interfaz para manejar mensajes recibidos
- `AzureQueueBackgroundProcessor<T>`: `BackgroundService` que escucha colas
- Reintentos automáticos configurados con Polly (3 intentos con backoff exponencial)

---

## 🗺️ Roadmap (próximas mejoras)

- Soporte para encabezados (`ApplicationProperties`)
- Publicación y programación de mensajes (scheduled delivery)
- Soporte para topics/subscriptions (opcional)
- Publicación oficial en NuGet.org

---

## 🧪 Testing y casos de uso

Pensado para microservicios backend:

- Worker Services
- APIs ASP.NET Core
- Microservicios desacoplados vía mensajería

Ideal para entornos con procesamiento asincrónico de eventos, orquestación y arquitectura basada en eventos.

---

✉️ Consultas, mejoras o feedback: abrí un issue o contactame.

