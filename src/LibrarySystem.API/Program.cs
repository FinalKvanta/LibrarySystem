using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LibrarySystem.Application.Contracts;
using LibrarySystem.Application.Services;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Infrastructure.Repositories;
using LibrarySystem.API.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/library-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting LibrarySystem API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Configure Kestrel and NetTcp
    builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));
    builder.WebHost.UseNetTcp(8090);

    // EF Core InMemory
    builder.Services.AddDbContext<LibraryDbContext>(options =>
        options.UseInMemoryDatabase("LibraryDb"));

    // Repositories
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IReaderRepository, ReaderRepository>();
    builder.Services.AddScoped<ILoanRepository, LoanRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Application services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<BookService>();
    builder.Services.AddScoped<ReaderService>();
    builder.Services.AddScoped<LoanService>();
    builder.Services.AddScoped<StatsService>();

    // CoreWCF
    builder.Services.AddServiceModelServices();
    builder.Services.AddServiceModelMetadata();

    // WCF service implementation
    builder.Services.AddScoped<LibraryServiceImpl>();

    var app = builder.Build();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        context.Database.EnsureCreated();
        SeedData.Initialize(context);
        Log.Information("Database seeded successfully.");
    }

    app.UseSerilogRequestLogging();

    app.UseServiceModel(serviceBuilder =>
    {
        serviceBuilder.AddService<LibraryServiceImpl>();

        serviceBuilder.AddServiceEndpoint<LibraryServiceImpl, ILibraryService>(
            new BasicHttpBinding(),
            "/LibraryService.svc");

        serviceBuilder.AddServiceEndpoint<LibraryServiceImpl, ILibraryService>(
            new NetTcpBinding(SecurityMode.None),
            "/LibraryService.svc");

        var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
        serviceMetadataBehavior.HttpGetEnabled = true;
        serviceMetadataBehavior.HttpGetUrl = new Uri("http://localhost:5000/LibraryService.svc");
    });

    Log.Information("LibrarySystem API is running.");
    Log.Information("HTTP endpoint: http://localhost:5000/LibraryService.svc");
    Log.Information("NetTcp endpoint: net.tcp://localhost:8090/LibraryService.svc");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
