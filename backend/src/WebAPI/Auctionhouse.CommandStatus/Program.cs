using Auctionhouse.CommandStatus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CommandStatusService>();

var serviceOpt = builder.Configuration.GetSection("CommandStatusServiceOptions").Get<CommandStatusServiceOptions>();
builder.Services.AddSingleton(serviceOpt);

var allowedOrigins = builder.Configuration.GetValue<string>("CORS:AllowedOrigins").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(allowedOrigins);
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Test")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/s/status/{commandId}", async (string commandId, CommandStatusService statusService) =>
{
    if(commandId.Length == 0)
    {
        return Results.BadRequest();
    }
    var status = await statusService.CheckStatus(commandId);
    return Results.Ok(status);
})
.WithName("GetCommandStatus");

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}