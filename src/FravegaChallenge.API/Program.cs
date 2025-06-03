using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebServices()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.Services.InitializeDatabase();
}

app.MapCarter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
var options = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(options.Value);

await app.RunAsync();

public partial class Program { }