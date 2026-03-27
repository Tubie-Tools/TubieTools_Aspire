var builder = DistributedApplication.CreateBuilder(args);

var publicApi = builder.AddProject<Projects.TubieTools_PublicAPI>("publicapi");

builder.AddProject<Projects.TubieTools_Aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints() 
    .WithReference(publicApi)
    .WaitFor(publicApi);

builder.AddProject<Projects.SanityCheque>("sanityCheque")
    .WithExternalHttpEndpoints()
    .WithReference(publicApi)
    .WaitFor(publicApi);

//test
//test2

builder.Build().Run();
