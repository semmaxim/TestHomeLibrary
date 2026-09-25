using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TestHomeLibrary.Infrastructure.Persistence;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private const int MaxAttempts = 30;
    private const int RetryDelaySeconds = 2;

    private readonly string _connectionString;
    private readonly string _scriptsDirectory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(string connectionString, string scriptsDirectory, ILogger<DatabaseInitializer> logger)
    {
        _connectionString = connectionString;
        _scriptsDirectory = scriptsDirectory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_scriptsDirectory))
        {
            _logger.LogWarning("Каталог скриптов БД не найден: {Directory}", _scriptsDirectory);
            return;
        }

        await ExecuteWithRetryAsync(async token =>
        {
            await EnsureDatabaseAsync(token);

            var scriptFiles = Directory.GetFiles(_scriptsDirectory, "*.sql")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(token);

            foreach (var scriptFile in scriptFiles)
            {
                foreach (var batch in SplitBatches(await File.ReadAllTextAsync(scriptFile, token)))
                {
                    await using var command = connection.CreateCommand();
                    command.CommandText = batch;
                    await command.ExecuteNonQueryAsync(token);
                }

                _logger.LogInformation("Выполнен скрипт {Script}", Path.GetFileName(scriptFile));
            }
        }, cancellationToken);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        var databaseName = GetInitialCatalog();
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "IF DB_ID(@DatabaseName) IS NULL EXEC('CREATE DATABASE [' + @DatabaseName + ']');";
        command.Parameters.AddWithValue("@DatabaseName", databaseName);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string GetInitialCatalog()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        return string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "TestHomeLibrary" : builder.InitialCatalog;
    }

    private async Task ExecuteWithRetryAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await action(cancellationToken);
                return;
            }
            catch (Exception ex) when (ex is SqlException or TimeoutException or IOException)
            {
                if (attempt >= MaxAttempts)
                {
                    _logger.LogError(ex, "Не удалось инициализировать базу данных после {Attempts} попыток.", attempt);
                    throw;
                }

                _logger.LogWarning(
                    "База данных недоступна (попытка {Attempt}/{MaxAttempts}): {Message}. Повтор через {Delay} с...",
                    attempt,
                    MaxAttempts,
                    ex.Message,
                    RetryDelaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(RetryDelaySeconds), cancellationToken);
            }
        }
    }

    private static IEnumerable<string> SplitBatches(string script)
    {
        var batches = new List<string>();
        var current = new List<string>();

        foreach (var line in script.Split('\n'))
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = string.Join('\n', current).Trim();
                if (batch.Length > 0)
                {
                    batches.Add(batch);
                }

                current.Clear();
            }
            else
            {
                current.Add(line);
            }
        }

        var tail = string.Join('\n', current).Trim();
        if (tail.Length > 0)
        {
            batches.Add(tail);
        }

        return batches;
    }
}
