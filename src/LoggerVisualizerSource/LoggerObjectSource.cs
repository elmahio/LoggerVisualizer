using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace LoggerVisualizerSource
{
    public class LoggerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            if (target.GetType() == typeof(LoggerFactory))
            {
                var field = GetPrivateField(target, "_providerRegistrations");
                var result = new LoggerModel
                {
                    Loggers = []
                };

                Type listType = field.GetType();
                if (typeof(IEnumerable).IsAssignableFrom(listType))
                {
                    // Cast the value to IEnumerable to access the elements
                    IEnumerable list = (IEnumerable)field;

                    foreach (var item in list)
                    {
                        var provider = GetPublicField(item, "Provider") as ILoggerProvider;
                        var providerTypeValue = provider?.GetType();

                        Logger loggerModel = new()
                        {
                            Name = providerTypeValue?.FullName,
                        };

                        switch (provider?.GetType().FullName)
                        {
                            case "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider":
                                {
                                    var optionsMonitor = GetPrivateField(provider, "_options");
                                    var options = GetPublicProperty(optionsMonitor, "CurrentValue");
                                    loggerModel = ConsoleOptions(options, loggerModel);
                                    break;
                                }
                            case "Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider":
                                {
                                    loggerModel = EventLogSettings(provider, loggerModel);
                                    break;
                                }
                            case "Elmah.Io.Extensions.Logging.ElmahIoLoggerProvider":
                                {
                                    loggerModel = ElmahIoOptions(provider, loggerModel);
                                    break;
                                }
                        }

                        result.Loggers.Add(loggerModel);
                    }
                }

                SerializeAsJson(outgoingData, result);
            }
            else if ((target.GetType().IsGenericType && target.GetType().GetGenericTypeDefinition() == typeof(Logger<>))
                || target.GetType().FullName == "Microsoft.Extensions.Logging.Logger")
            {
                ILogger logger = target.GetType().FullName == "Microsoft.Extensions.Logging.Logger"
                    ? target as ILogger
                    : GetPrivateField(target, "_logger") as ILogger;

                if (logger == null) return;

                var loggers = GetPublicProperty(logger, "Loggers") as Array;
                var messageLoggers = GetPublicProperty(logger, "MessageLoggers") as Array;

                // Microsoft.Extensions.Loggging 8 has the name in _categoryName
                var name = GetPrivateField(logger, "_categoryName")?.ToString();

                var minLevel = (CalculateEnabledLogLevel(logger) ?? LogLevel.None).ToString();
                var enabled = (minLevel != null).ToString();

                var result = new LoggerModel
                {
                    Name = name,
                    MinLevel = minLevel,
                    Enabled = enabled,
                    Loggers = []
                };
                foreach (var l in loggers)
                {
                    // If name was not resolved from _categoryName use the Category property from the first logger
                    if (string.IsNullOrWhiteSpace(result.Name))
                    {
                        result.Name = GetPublicProperty(l, "Category")?.ToString();
                    }
                    var providerTypeValue = GetPublicProperty(l, "ProviderType") as Type;
                    var internalLogger = GetPublicProperty(l, "Logger") as ILogger;
                    var messageLogger = FirstOrNull(messageLoggers, internalLogger);
                    Logger loggerModel = new()
                    {
                        Name = providerTypeValue.FullName,
                        ExternalScope = GetPublicProperty(l, "ExternalScope").ToString(),
                        MinLevel = (GetPublicProperty(messageLogger, "MinLevel") ?? LogLevel.None).ToString(),
                    };

                    switch (providerTypeValue.FullName)
                    {
                        case "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider":
                            {
                                var options = GetNonPublicProperty(internalLogger, "Options");
                                loggerModel = ConsoleOptions(options, loggerModel);
                                break;
                            }
                        case "Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider":
                            {
                                loggerModel = EventLogSettings(internalLogger, loggerModel);
                                break;
                            }
                        case "Elmah.Io.Extensions.Logging.ElmahIoLoggerProvider":
                            {
                                loggerModel = ElmahIoOptions(internalLogger, loggerModel);
                                break;
                            }
                    }

                    result.Loggers.Add(loggerModel);
                }

                SerializeAsJson(outgoingData, result);
            }
        }

        private Logger ElmahIoOptions(object objWithOptions, Logger loggerModel)
        {
            var options = GetPrivateField(objWithOptions, "_options");
            loggerModel.ApiKey = GetPublicProperty(options, "ApiKey")?.ToString();
            var logId = GetPublicProperty(options, "LogId") as Guid?;
            loggerModel.LogId = logId == Guid.Empty ? "" : logId?.ToString();
            return loggerModel;
        }

        private Logger EventLogSettings(object objWithSettings, Logger loggerModel)
        {
            var settings = GetPrivateField(objWithSettings, "_settings");
            loggerModel.LogName = GetPublicProperty(settings, "LogName")?.ToString();
            loggerModel.MachineName = GetPublicProperty(settings, "MachineName")?.ToString();
            loggerModel.SourceName = GetPublicProperty(settings, "SourceName")?.ToString();
            return loggerModel;
        }

        private Logger ConsoleOptions(object options, Logger loggerModel)
        {
            loggerModel.FormatterName = GetPublicProperty(options, "FormatterName")?.ToString();
            loggerModel.TimestampFormat = GetPublicProperty(options, "TimestampFormat")?.ToString();
            loggerModel.QueueFullMode = GetPublicProperty(options, "QueueFullMode")?.ToString();
            loggerModel.DisableColors = GetPublicProperty(options, "DisableColors")?.ToString();
            loggerModel.LogToStandardErrorThreshold = GetPublicProperty(options, "LogToStandardErrorThreshold")?.ToString();
            loggerModel.MaxQueueLength = GetPublicProperty(options, "MaxQueueLength")?.ToString();
            loggerModel.UseUtcTimestamp = GetPublicProperty(options, "UseUtcTimestamp")?.ToString();
            return loggerModel;
        }

        static object FirstOrNull(Array messageLoggers, ILogger logger)
        {
            if (messageLoggers is null || messageLoggers.Length == 0)
            {
                return null;
            }

            foreach (object item in messageLoggers)
            {
                var theLogger = GetPublicProperty(item, "Logger") as ILogger;
                if (theLogger == logger)
                {
                    return item;
                }
            }

            return null;
        }

        private static LogLevel? CalculateEnabledLogLevel(ILogger logger)
        {
            if (logger == null)
            {
                return null;
            }

            var logLevels = new ReadOnlySpan<LogLevel>(
            [
                LogLevel.Critical,
                LogLevel.Error,
                LogLevel.Warning,
                LogLevel.Information,
                LogLevel.Debug,
                LogLevel.Trace,
            ]);

            LogLevel? minimumLevel = null;

            // Check log level from highest to lowest. Report the lowest log level.
            foreach (LogLevel logLevel in logLevels)
            {
                if (!logger.IsEnabled(logLevel))
                {
                    break;
                }

                minimumLevel = logLevel;
            }

            return minimumLevel;
        }

        private static object GetPublicProperty(object obj, string name)
        {
            var propertyInfo = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) return null;
            var theValue = propertyInfo.GetValue(obj);
            return theValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "")]
        private static object GetNonPublicProperty(object obj, string name)
        {
            var propertyInfo = obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null) return null;
            var theValue = propertyInfo.GetValue(obj);
            return theValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "")]
        private static object GetPrivateField(object obj, string name)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return null;
            var theValue = fieldInfo.GetValue(obj);
            return theValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "")]
        private static object GetPublicField(object obj, string name)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo == null) return null;
            var theValue = fieldInfo.GetValue(obj);
            return theValue;
        }

    }
}
