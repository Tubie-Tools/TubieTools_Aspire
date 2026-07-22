using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//env variable password
// Create a ParameterResource for a password
var secretpassword = builder.AddParameter("dbpassword", secret: true);

var password = builder.AddParameter("SqlAdminPassword","My$ecureP@ssw0rd",false,true); // Or omit to auto-generate

// Add a SQL Server container for local development
// Except my container is garbage, fallback to local SSMS .
//var sqldb = builder.AddSqlServer("sql", secretpassword)
//    .WithLifetime(ContainerLifetime.Persistent)
//    .WithDataBindMount(source: @"C:\SqlServer\Data")
//    .WithEndpoint(port: 6033, targetPort: 1433, name: "ssms")
//    .WithContainerName("sqlserver")
//    .WithDataVolume()
//    .WithEnvironment("ACCEPT_EULA", "Y");

// Optionally add a database to the SQL Server
//var db = sqldb.AddDatabase("MyDatabase");

var publicApi = builder.AddProject<Projects.TubieTools_PublicAPI>("publicapi");
//.WithReference(db);
builder.AddProject<Projects.TubieTools_SentimentModel_WebApi>("sentimentapi");
builder.AddProject<Projects.TubieTools_SentimentModel_ProductApi>("sentimentproductapi");
builder.AddProject<Projects.TubieTools_Forecasting_API>("forecastingapi");

// start tensorflow
//builder.AddProject<Projects.TubieTools_TensorFlow>("tensorflow");

builder.AddProject<Projects.TubieTools_Aspire_Web>("webfrontend")
    .WithExternalHttpEndpoints() 
    .WithReference(publicApi)
    .WaitFor(publicApi);

/*builder.AddProject<Projects.SanityCheque>("sanityCheque")
    .WithExternalHttpEndpoints()
    .WithReference(publicApi)
    .WaitFor(publicApi);
*/
//test
//test2

builder.Build().Run();
