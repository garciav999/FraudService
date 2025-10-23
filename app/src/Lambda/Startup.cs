using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Application.Interface;
using Application.Commands;

public class Startup
{
    public IServiceProvider Configure()
    {
        var services = new ServiceCollection();

        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        var conn = configuration.GetConnectionString("DefaultConnection")
                   ?? configuration["ConnectionStrings:DefaultConnection"]
                   ?? System.Environment.GetEnvironmentVariable("DefaultConnection");

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Database connection string not configured (DefaultConnection).");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(conn)
        );

        services.AddScoped<ITransactionDayRepository, TransactionDayRepository>();
        services.AddScoped<TransactionDayCommands>();

        return services.BuildServiceProvider();
    }
}
