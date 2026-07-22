var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter(
    "postgres-password",
    secret: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithImage("postgis/postgis")
    .WithImageTag("17-3.5");

var taxidb = postgres.AddDatabase("taxidb");

builder.AddProject<Projects.Taxi_Web_Api>("api")
    .WithReference(taxidb)
    .WaitFor(taxidb);

builder.Build().Run();