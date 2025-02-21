var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.KaruRestauranteWebApp_ApiService>("apiservice");

builder.AddProject<Projects.KaruRestauranteWebApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
