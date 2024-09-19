using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.Extensibility;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;

namespace LoggerVisualizer.Models
{
    [DataContract]
    public class LoggerRootViewModel
    {
        [DataMember]
        public string? Name { get; set; }

        [DataMember]
        public string? Enabled { get; set; }

        [DataMember]
        public string? MinLevel { get; set; }

        [DataMember]
        public string MinLevelColor { get; set; } = "#6c757d";

        [DataMember]
        public ObservableCollection<LoggerViewModel> Loggers { get; set; } = [];
    }

    [DataContract]
    public class LoggerViewModel
    {
        private static readonly HttpClient httpClient = new();

        [DataMember]
        public string? ShortName { get; set; }

        [DataMember]
        public string? Name { get; set; }

        [DataMember]
        public string? ExternalScope { get; set; }

        [DataMember]
        public string? MinLevel { get; set; }
        [DataMember]
        public string MinLevelColor { get; set; } = "#6c757d";

        [DataMember]
        public string? FormatterName { get; set; }

        [DataMember]
        public string? DisableColors { get; set; }

        [DataMember]
        public string? LogToStandardErrorThreshold { get; set; }

        [DataMember]
        public string? MaxQueueLength { get; set; }

        [DataMember]
        public string? QueueFullMode { get; set; }

        [DataMember]
        public string? TimestampFormat { get; set; }

        [DataMember]
        public string? UseUtcTimestamp { get; set; }

        [DataMember]
        public string? MachineName { get; set; }

        [DataMember]
        public string? SourceName { get; set; }

        [DataMember]
        public string? LogName { get; set; }

        [DataMember]
        public string? ApiKey { get; set; }

        [DataMember]
        public string? LogId { get; set; }

        [DataMember]
        public Visibility IsConsole { get; set; } = Visibility.Collapsed;

        [DataMember]
        public Visibility IsElmahIo { get; set; } = Visibility.Collapsed;

        [DataMember]
        public Visibility IsEventLog { get; set; } = Visibility.Collapsed;

        [DataMember]
        public Visibility IsEventSource { get; set; } = Visibility.Collapsed;

        [DataMember]
        public IAsyncCommand Diagnose { get; set; } = new DiagnoseCommand();

        private sealed class DiagnoseCommand : IAsyncCommand
        {
            public bool CanExecute => true;

            public async Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
            {
                if (parameter is not LoggerViewModel model) return;

                try
                {
                    var result = await httpClient.GetStringAsync($"https://api.elmah.io/v3/logs/{model.LogId}/_diagnose?api_key={model.ApiKey}", cancellationToken);
                    MessageBox.Show(result);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        [DataMember]
        public bool CanBrowse { get; set; } = false;

        [DataMember]
        public bool CanDiagnose { get; set; } = false;

        [DataMember]
        public IAsyncCommand Browse { get; set; } = new BrowseCommand();

        private sealed class BrowseCommand : IAsyncCommand
        {
            public bool CanExecute => true;

            public Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
            {
                if (parameter is not LoggerViewModel model) return Task.CompletedTask;

                if (!string.IsNullOrWhiteSpace(model.LogId))
                {
                    Process.Start(new ProcessStartInfo { FileName = $"https://app.elmah.io/errorlog/search/?logId={model.LogId}", UseShellExecute = true });
                }

                return Task.CompletedTask;
            }
        }

        [DataMember]
        public IAsyncCommand ProcessLauncher { get; set; } = new ProcessLauncherCommand();

        private sealed class ProcessLauncherCommand : IAsyncCommand
        {
            public bool CanExecute => true;

            public Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
            {
                var command = parameter as string;
                if ((string.IsNullOrWhiteSpace(command))) return Task.CompletedTask;

                Process.Start(new ProcessStartInfo { FileName = command, UseShellExecute = true });

                return Task.CompletedTask;
            }
        }
    }
}
