using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace CustomerSatisfaction;

class Program
{
    static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddServiceDefaults();

        var endpointConfiguration = new EndpointConfiguration("CustomerSatisfaction");

        var connectionString = builder.Configuration.GetConnectionString("transport");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
        var routing = endpointConfiguration.UseTransport(transport);

        endpointConfiguration.EnableInstallers();

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        builder.UseNServiceBus(endpointConfiguration);

        return builder.Build().RunAsync();
    }
}