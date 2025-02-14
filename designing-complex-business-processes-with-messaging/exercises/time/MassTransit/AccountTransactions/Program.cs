using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Client;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace AccountTransactions;

class Program
{
    static Task Main()
    {
        var builder = Host.CreateApplicationBuilder();
        
        builder.AddServiceDefaults();

        var connectionString = builder.Configuration.GetConnectionString("transport");
        builder.Services.AddOptions<SqlTransportOptions>()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
            });

        builder.Services.AddPostgresMigrationHostedService(options =>
        {
            options.CreateDatabase = true;
        });

        builder.Services.AddMassTransit(x =>
        {
            // Using InMemory repository to simplify the configuration, but this could benefit from using the MassTransit.EntityFrameworkCore package to store data in Postgres
            x.AddSagaStateMachine<AccountInterestPolicy, AccountInterestPolicyState>().InMemoryRepository();
            
            x.AddConfigureEndpointsCallback((provider, name, cfg) =>
            {
                if (cfg is ISqlReceiveEndpointConfigurator sql)
                {
                    sql.LockDuration = TimeSpan.FromMinutes(10);
                    sql.SetReceiveMode(SqlReceiveMode.Normal);
                }
            });

            x.AddSqlMessageScheduler();

            x.UsingPostgres((context, cfg) =>
            {
                cfg.UseSqlMessageScheduler();
                
                cfg.ReceiveEndpoint("AccountTransactions", e =>
                {
                    // configure any required middleware components next
                    e.UseMessageRetry(r => r.Interval(5, 1000));
            
                    // If a persister is configured, add:
                    // e.ConfigureSaga<AccountInterestPolicy>(context);
                });
                
                cfg.ConfigureEndpoints(context);
            });
            
           
        });
        
        builder.Services.AddHostedService<TransactionsSenderService>();

        return builder.Build().RunAsync();
    }
}