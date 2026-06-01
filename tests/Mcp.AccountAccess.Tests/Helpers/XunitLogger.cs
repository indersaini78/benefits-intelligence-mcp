using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Mcp.AccountAccess.Tests.Helpers;

public sealed class XunitLogger<T>(ITestOutputHelper output) : ILogger<T>
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _messages.Add(message);
        output.WriteLine($"[{logLevel}] {message}");
    }
}
