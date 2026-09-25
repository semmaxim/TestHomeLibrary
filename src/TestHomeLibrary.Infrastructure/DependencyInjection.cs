using Microsoft.Extensions.DependencyInjection;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Domain.Interfaces;
using TestHomeLibrary.Infrastructure.Html;
using TestHomeLibrary.Infrastructure.Persistence;

namespace TestHomeLibrary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string? scriptsDirectory = null)
    {
        services.AddSingleton<IDatabaseInitializer>(provider => new DatabaseInitializer(
            connectionString,
            scriptsDirectory ?? Path.Combine(AppContext.BaseDirectory, "db"),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DatabaseInitializer>>()));

        services.AddScoped<IBookRepository>(_ => new DapperBookRepository(connectionString));
        services.AddSingleton<ITableOfContentsConverter, HtmlTableOfContentsConverter>();
        return services;
    }
}
