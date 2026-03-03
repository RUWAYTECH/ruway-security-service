using Ruway.Security.Subscription.Hub.Services;
using Ruway.Security.Subscription.Hub.Extensions;
using Serilog;

namespace Ruway.Security.Subscription.Hub;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Configurar Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/ruway-security-subscription-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Iniciando Ruway Security Subscription Hub");
            
            var builder = Host.CreateApplicationBuilder(args);
            
            // Configurar el servicio para ejecutarse como un servicio de Windows
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "Ruway Security Subscription Hub";
            });

            // Configurar logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            // Registrar el servicio principal
            builder.Services.AddHostedService<EventSubscriptionService>();
            
            // Configurar servicios de eventos
            builder.Services.AddEventServices(builder.Configuration);

            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "El servicio se detuvo de manera inesperada");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}