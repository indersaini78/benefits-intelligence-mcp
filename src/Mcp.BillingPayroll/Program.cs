using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Infrastructure.Persistence.BillingPayroll;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<BillingPayrollDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BenefitsDb")));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.Run();
