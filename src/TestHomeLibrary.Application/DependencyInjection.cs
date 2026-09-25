using Microsoft.Extensions.DependencyInjection;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Application.Services;

namespace TestHomeLibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        return services;
    }
}
