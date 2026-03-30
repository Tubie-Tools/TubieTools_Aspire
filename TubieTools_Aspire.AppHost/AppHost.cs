using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add a SQL Server container for local development
var sqldb = builder.AddSqlServer("sanitycheque");//.WithPassword(passwordFromKeyVault);

// Optionally add a database to the SQL Server
var db = sqldb.AddDatabase("MyDatabase");

var publicApi = builder.AddProject<Projects.TubieTools_PublicAPI>("publicapi").
    WithReference(db);


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
