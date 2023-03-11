﻿using Microsoft.Extensions.Logging;
using System.Text;

namespace Xunit.DependencyInjection.Logging;

public class XunitTestOutputLogger : ILogger
{
    private const string LogLevelPadding = ": ";
    private static readonly string MessagePadding;
    private static readonly string NewLineWithMessagePadding;

    [ThreadStatic]
    private static StringBuilder? _logBuilder;

    static XunitTestOutputLogger()
    {
        var logLevelString = GetLogLevelString(LogLevel.Information);
        MessagePadding = new(' ', logLevelString.Length + LogLevelPadding.Length);
        NewLineWithMessagePadding = Environment.NewLine + MessagePadding;
    }

    private readonly ITestOutputHelperAccessor _accessor;
    private readonly string _categoryName;
    private readonly Func<string, LogLevel, bool> _filter;

    public XunitTestOutputLogger(ITestOutputHelperAccessor accessor, string categoryName, Func<string, LogLevel, bool> filter)
    {
        _accessor = accessor;
        _categoryName = categoryName;
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public bool IsEnabled(LogLevel logLevel) => _filter(_categoryName, logLevel);

    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!_filter(_categoryName, logLevel)) return;

        if (formatter == null) throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
            WriteMessage(logLevel, _categoryName, eventId.Id, message, exception);
    }

    public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception? exception)
    {
        var logBuilder = _logBuilder;
        _logBuilder = null;

        logBuilder ??= new();

        var logLevelString = GetLogLevelString(logLevel);
        // category and event id
        logBuilder.Append(LogLevelPadding);
        logBuilder.Append(logName);
        logBuilder.Append('[');
        logBuilder.Append(eventId);
        logBuilder.AppendLine("]");

        if (!string.IsNullOrEmpty(message))
        {
            // message
            logBuilder.Append(MessagePadding);

            var len = logBuilder.Length;
            logBuilder.AppendLine(message);
            logBuilder.Replace(Environment.NewLine, NewLineWithMessagePadding, len, message.Length);
        }

        if (exception != null) logBuilder.AppendLine(exception.ToString());

        if (logBuilder.Length > 0)
        {
            if (!string.IsNullOrEmpty(logLevelString)) logBuilder.Insert(0, logLevelString);

            try
            {
                _accessor.Output?.WriteLine(logBuilder.ToString().TrimEnd());
            }
            catch (InvalidOperationException) //There is no currently active test case.
            {
                // ignored
            }
        }

        logBuilder.Clear();

        if (logBuilder.Capacity > 1024) logBuilder.Capacity = 1024;

        _logBuilder = logBuilder;
    }

    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
    };

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        private NullScope() { }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
