var builder = DistributedApplication.CreateBuilder(args);

var transport = builder.AddRabbitMQ("transport")
    .WithManagementPlugin();

var database = builder.AddPostgres("database")
    // NOTE: This is needed as the call to AddDatabase below
    // does not actually create the database
    .WithEnvironment("POSTGRES_DB", "transactions-db")
    .WithPgWeb()
    .WithImage("library/postgres", "15.8");

var transactionsDb = database.AddDatabase("transactions-db");

builder.AddProject<Projects.AccountTransactions>("account-transactions")
    .WithReference(transport)
    .WaitFor(transport)
    .WithReference(transactionsDb)
    .WaitFor(transactionsDb);

builder.Build().Run();