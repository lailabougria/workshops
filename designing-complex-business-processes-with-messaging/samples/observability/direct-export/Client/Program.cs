﻿using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using OpenTelemetry.Resources;

namespace Client;

public class Program
{
    public static void Main(string[] args)
    {
        var endpointName = "Client";
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                // Register the message sender service
                services.AddHostedService<MessageSenderService>();
                   
                // Enables capturing experimental OpenTelemetry from the Azure SDK
                // AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

                var appInsightsConnString = Environment.GetEnvironmentVariable("Workshop_AppInsights_ConnectionString");
                services.AddOpenTelemetry()
                    .ConfigureResource(resourceBuilder => resourceBuilder.AddService(endpointName))
                    .WithTracing(tracingBuilder => tracingBuilder
                        .AddSource("NServiceBus.*")
                        //.AddSource("Azure.*")
                        .AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnString;
                        })
                    )
                    .WithMetrics(metricsBuilder => metricsBuilder
                        .AddMeter("NServiceBus.*")
                        .AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnString;
                        })
                    );

                // connect traces with logs
                services.AddLogging(loggingBuilder =>
                    loggingBuilder.AddOpenTelemetry(otelLoggerOptions =>
                    {
                        otelLoggerOptions.IncludeFormattedMessage = true;
                        otelLoggerOptions.IncludeScopes = true;
                        otelLoggerOptions.ParseStateValues = true;
                        otelLoggerOptions.AddAzureMonitorLogExporter(options =>
                            options.ConnectionString = appInsightsConnString
                        );
                    }).AddConsole()
                );
            })
            .UseNServiceBus(_ =>
            {
                var endpointConfiguration = new EndpointConfiguration(endpointName);
                endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                endpointConfiguration.UsePersistence<LearningPersistence>();

                var connectionString = Environment.GetEnvironmentVariable("Workshop_ServiceBus_ConnectionString");
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Please specify a connection string for the broker.");
                var transport = new AzureServiceBusTransport(connectionString, TopicTopology.Default);
                   
                var routing = endpointConfiguration.UseTransport(transport);
                routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");
                routing.RouteToEndpoint(typeof(ChargeOrder), "Payments");
                   
                endpointConfiguration.EnableInstallers();
                endpointConfiguration.EnableOpenTelemetry();

                return endpointConfiguration;
            }).Build();

        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
        Console.Title = hostEnvironment.ApplicationName;
        host.Run();
    }
}