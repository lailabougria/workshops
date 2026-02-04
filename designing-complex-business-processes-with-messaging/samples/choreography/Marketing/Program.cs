using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Marketing;

class Program
{
    const string EndpointName = "Marketing";

    private static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        var endpointConfiguration = new EndpointConfiguration(EndpointName);

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