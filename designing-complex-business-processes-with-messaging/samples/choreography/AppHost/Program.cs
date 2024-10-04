using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var transport = builder.AddRabbitMQ("transport")
    .WithManagementPlugin()
    .WithHealthCheck();

builder.AddProject<Projects.Marketing>("marketing")
    .WithReference(transport)
    .WaitFor(transport);

var payments = builder.AddProject<Projects.Payments>("payments")
    .WithReference(transport)
    .WaitFor(transport);

var sales = builder.AddProject<Projects.Sales>("sales")
    .WithReference(transport)
    .WaitFor(transport);

builder.AddProject<Projects.Shipping>("shipping")
    .WithReference(transport)
    .WaitFor(transport);

var database = builder.AddPostgres("database")
    // NOTE: This is needed as the call to AddDatabase below
    // does not actually create the database
    .WithEnvironment("POSTGRES_DB", "packing-db")
    .WithPgWeb()
    .WithImage("library/postgres", "15.8");

var packingDb = database.AddDatabase("packing-db")
    .WithHealthCheck();

var pickingAndPacking = builder.AddProject<Projects.PickingAndPacking>("picking-and-packing")
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(packingDb)
    .WaitFor(packingDb);

builder.AddProject<Projects.Client>("client")
    .WithReference(transport)
    .WaitFor(transport)
    .WaitFor(sales)
    .WaitFor(payments)
    .WaitFor(pickingAndPacking);

builder.Build().Run();