using Mimisbrunnr.Web.Host;
using Skidbladnir.Modules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSkidbladnirModules<StartupModule>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();