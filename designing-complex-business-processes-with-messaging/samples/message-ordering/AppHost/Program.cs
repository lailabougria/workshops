#region app-host
var builder = DistributedApplication.CreateBuilder(args);

var transportUserName = builder.AddParameter("transportUserName", "guest", secret: true);
var transportPassword = builder.AddParameter("transportPassword", "guest", secret: true);

var transport = builder.AddRabbitMQ("transport", transportUserName, transportPassword)
    .WithManagementPlugin(15672)
    .WithUrlForEndpoint("management", url => url.DisplayText = "RabbitMQ Management");

transportUserName.WithParentRelationship(transport);
transportPassword.WithParentRelationship(transport);

builder.AddProject<Projects.CustomerSatisfaction>("customer-satisfaction")
    .WithReference(transport)
    .WaitFor(transport);

builder.AddProject<Projects.Payments>("payments")
    .WithReference(transport)
    .WaitFor(transport);

var sales = builder.AddProject<Projects.Sales>("sales")
    .WithReference(transport)
    .WaitFor(transport);

builder.AddProject<Projects.Client>("client")
    .WithReference(transport)
    .WaitFor(transport)
    .WaitFor(sales);

var database = builder.AddPostgres("database")
    // NOTE: This is needed as the call to AddDatabase below
    // does not actually create the database
    .WithEnvironment("POSTGRES_DB", "shipping-db")
    .WithPgWeb()
    .WithImage("library/postgres", "15.8");

var shippingDb = database.AddDatabase("shipping-db");

builder.AddProject<Projects.Shipping>("shipping")
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(shippingDb)
    .WaitFor(shippingDb);

builder.Build().Run();
#endregion