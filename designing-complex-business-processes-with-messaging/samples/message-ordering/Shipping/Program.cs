using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;
using NServiceBus;
using System.Threading.Tasks;

namespace Shipping;

class Program
{
    static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        var endpointConfiguration = new EndpointConfiguration("Shipping");
        endpointConfiguration.EnableOpenTelemetry();

        builder.AddServiceDefaults();

        var connectionString = builder.Configuration.GetConnectionString("transport");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
        endpointConfiguration.UseTransport(transport);
        endpointConfiguration.EnableOutbox();

        var persistenceConnection = builder.Configuration.GetConnectionString("shipping-db");
        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new NpgsqlConnection(persistenceConnection);
            });

        var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
        dialect.JsonBParameterModifier(
            modifier: parameter =>
            {
                var npgsqlParameter = (NpgsqlParameter)parameter;
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            });

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();

        builder.UseNServiceBus(endpointConfiguration);

        return builder.Build().RunAsync();
    }
}