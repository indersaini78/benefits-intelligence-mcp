using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Infrastructure.Persistence.Eligibility;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<EligibilityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BenefitsDb")));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

//app.MapMcp();

app.Run();
