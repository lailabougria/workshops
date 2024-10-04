using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;
using PickingAndPacking.InternalContracts;

namespace PickingAndPacking;

class Program
{
    const string EndpointName = "PickingAndPacking";

    private static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.EnableOpenTelemetry();

        builder.AddServiceDefaults();

        var connectionString = builder.Configuration.GetConnectionString("transport");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
        var routing = endpointConfiguration.UseTransport(transport);
        routing.RouteToEndpoint(typeof(ProductRestocked), "PickingAndPacking");

        var persistenceConnection = builder.Configuration.GetConnectionString("packing-db");
        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () => { return new NpgsqlConnection(persistenceConnection); });

        var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
        dialect.JsonBParameterModifier(
            modifier: parameter =>
            {
                var npgsqlParameter = (NpgsqlParameter)parameter;
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            });

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.EnableOutbox();

        builder.UseNServiceBus(endpointConfiguration);

        return builder.Build().RunAsync();
    }
}