﻿using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using Microsoft.Extensions.Logging;
using LoggerVisualizerSource;
using System.Collections.ObjectModel;
using LoggerVisualizer.Models;
using System.Windows;

namespace LoggerVisualizer
{
    [VisualStudioContribution]
    internal class LoggerDebuggerVisualizerProvider : DebuggerVisualizerProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ConstantExpressionEvaluator", "CEE0027:String not localized", Justification = "")]
        public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration => new(
            new VisualizerTargetType("Logger Visualizer", "Microsoft.Extensions.Logging.Logger, Microsoft.Extensions.Logging, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60"),
            new VisualizerTargetType("Logger Visualizer", typeof(Logger<>)),
            new VisualizerTargetType("Logger Factory Visualizer", typeof(LoggerFactory)))
        {
            VisualizerObjectSourceType = new(typeof(LoggerObjectSource)),
        };
        public override async Task<IRemoteUserControl> CreateVisualizerAsync(VisualizerTarget visualizerTarget, CancellationToken cancellationToken)
        {
            LoggerModel? model;
            try
            {
                model = await visualizerTarget
                    .ObjectSource
                    .RequestDataAsync<LoggerModel?>(jsonSerializer: null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                throw;
            }

            return await Task.FromResult<IRemoteUserControl>(new LoggerVisualizerUserControl(ToViewModel(model)));
        }

        private LoggerRootViewModel? ToViewModel(LoggerModel? logger)
        {
            if (logger == null) return null;
            var viewModel = new LoggerRootViewModel
            {
                Name = logger.Name,
                Enabled = logger.Enabled,
                MinLevel = logger.MinLevel,
                MinLevelColor = MinLevelColor(logger.MinLevel),
                Loggers = new ObservableCollection<LoggerViewModel>(logger.Loggers.Select(l => new LoggerViewModel
                {
                    Name = l.Name,
                    ExternalScope = l.ExternalScope,
                    MinLevel = l.MinLevel,
                    MinLevelColor = MinLevelColor(l.MinLevel),
                    ShortName = l.Name[(1 + l.Name.LastIndexOf('.'))..].Replace("LoggerProvider", string.Empty),
                    // Console
                    IsConsole = l.Name == "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider" ? Visibility.Visible : Visibility.Collapsed,
                    FormatterName = l.FormatterName,
                    DisableColors = l.DisableColors,
                    LogToStandardErrorThreshold = l.LogToStandardErrorThreshold,
                    MaxQueueLength = l.MaxQueueLength,
                    QueueFullMode = l.QueueFullMode,
                    TimestampFormat = l.TimestampFormat,
                    UseUtcTimestamp = l.UseUtcTimestamp,
                    // EventLog
                    IsEventLog = l.Name == "Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider" ? Visibility.Visible : Visibility.Collapsed,
                    MachineName = l.MachineName,
                    SourceName = l.SourceName,
                    LogName = l.LogName,
                    // EventSource
                    IsEventSource = l.Name == "Microsoft.Extensions.Logging.EventSource.EventSourceLoggerProvider" ? Visibility.Visible : Visibility.Collapsed,
                    // ElmahIo
                    IsElmahIo = l.Name == "Elmah.Io.Extensions.Logging.ElmahIoLoggerProvider" ? Visibility.Visible : Visibility.Collapsed,
                    ApiKey = l.ApiKey,
                    LogId = l.LogId,
                    CanBrowse = !string.IsNullOrWhiteSpace(l.LogId) && Guid.TryParse(l.LogId, out Guid _),
                    CanDiagnose = !string.IsNullOrWhiteSpace(l.LogId) && Guid.TryParse(l.LogId, out Guid _)
                        && !string.IsNullOrWhiteSpace(l.ApiKey) && l.ApiKey.Length == 32,
                })),
            };

            return viewModel;
        }

        private static string MinLevelColor(string minLevel)
        {
            return minLevel switch
            {
                "Critical" => "#993636",
                "Debug" => "#95c1ba",
                "Error" => "#e6614f",
                "Information" => "#0da58e",
                "Trace" => "#ccc",
                "Warning" => "#ffc936",
                _ => "#6c757d",
            };
        }
    }
}
