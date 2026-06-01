using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Infrastructure.Persistence.Enrollment;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<EnrollmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BenefitsDb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.Run();


