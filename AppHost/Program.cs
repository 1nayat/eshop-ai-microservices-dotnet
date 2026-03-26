var builder = DistributedApplication.CreateBuilder(args);

// ==========================
// PARAMETERS (User Secrets)
// ==========================
builder.AddParameter("sqlserver-password");
builder.AddParameter("postgres-password");
builder.AddParameter("cache-password");
builder.AddParameter("vectordb-Key");


// ==========================
// BACKING SERVICES
// ==========================

// 🔵 PostgreSQL
var postgres = builder
    .AddPostgres("postgres")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", builder.Configuration["Parameters:postgres-password"])
    .WithPgAdmin(pgAdmin =>
        pgAdmin.WithUrlForEndpoint("http", url =>
            url.DisplayText = "PostgreDB Browser"))
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var catalogdb = postgres.AddDatabase("catalogdb");


// 🟠 Redis
var cache = builder
    .AddRedis("cache")
    .WithEnvironment("REDIS_PASSWORD", builder.Configuration["Parameters:cache-password"])
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);


// 🔴 SQL Server
var sqlServer = builder
    .AddSqlServer("sqlserver")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SA_PASSWORD", builder.Configuration["Parameters:sqlserver-password"])
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var orderdb = sqlServer.AddDatabase("orderdb");


// 🟣 Qdrant (Vector DB)
var vectorDb = builder
    .AddQdrant("vectordb")
    .WithEnvironment("QDRANT__SERVICE__API_KEY", builder.Configuration["Parameters:vectordb-Key"])
    .WithDataVolume();


// ==========================
// MICROSERVICES
// ==========================

// 🟢 Catalog Service
var catalog = builder
    .AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogdb)
    .WaitFor(catalogdb)
    .WithReference(vectorDb)
    .WaitFor(vectorDb);


// 🟡 Basket Service
var basket = builder
    .AddProject<Projects.Basket>("basket")
    .WithReference(cache)
    .WaitFor(cache);


// 🔵 Ordering Service
var ordering = builder
    .AddProject<Projects.Ordering>("ordering")
    .WithReference(orderdb)
    .WaitFor(orderdb);


// Basket → Ordering dependency
basket
    .WithReference(ordering)
    .WaitFor(ordering);


// 🌐 Web App
var webapp = builder
    .AddProject<Projects.WebApp>("webapp")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
    .WithReference(catalog)
    .WithReference(basket)
    .WithReference(ordering)
    .WaitFor(catalog)
    .WaitFor(basket)
    .WaitFor(ordering);


// ==========================
// RUN
// ==========================
builder.Build().Run();