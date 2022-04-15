using Microsoft.AspNetCore.SpaServices;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Host;
using Skidbladnir.Modules;
using VueCliMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSkidbladnirModules<StartupModule>(configuration =>
{
    var mongoConfig = builder.Configuration.GetSection("Storage").Get<MongoDbStoreModuleConfiguration>();
    configuration.Add(mongoConfig);
}, builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSpaStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapToVueCliProxy(
    "{*path}",
    new SpaOptions { SourcePath = "ClientApp"},
    npmScript: "serve",
    regex: "Compiled successfully",
    forceKill: true
);

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
});

app.Run();