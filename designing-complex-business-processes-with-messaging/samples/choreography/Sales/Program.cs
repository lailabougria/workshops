using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Sales;

class Program
{
    const string EndpointName = "Sales";

    private static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.EnableOpenTelemetry();

        builder.AddServiceDefaults();

        var connectionString = builder.Configuration.GetConnectionString("transport");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
        endpointConfiguration.UseTransport(transport);

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();

        builder.UseNServiceBus(endpointConfiguration);

        return builder.Build().RunAsync();
    }
}