using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.Scheme,
                    null);

builder.Services.AddControllers().AddApplicationPart(
    Assembly.Load("Adapter.QuartzTimeTaskService.AuctionEndScheduler"));

var app = builder.Build();

var actions = app.Services.GetService<IActionDescriptorCollectionProvider>();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }