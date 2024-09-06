using Elmah.Io.Client;
using System.Security.Principal;
using System.Windows;

namespace LoggerVisualizer
{
    /// <summary>
    /// Reports anonymous usage through elmah.io.
    /// This class is heavily inspired from this: https://github.com/ErikEJ/EFCorePowerTools/blob/master/src/GUI/Shared/Helpers/Telemetry.cs by Erik Ejlskov Jensen
    /// </summary>
    public static class Telemetry
    {
        private static IElmahioAPI elmahIoClient;

        public static bool Enabled { get; set; }

        public static void Initialize(string? version)
        {
            if (elmahIoClient != null)
            {
                return;
            }

            elmahIoClient = ElmahioAPI.Create("9beeaf23f3f44e49bb6ca059824c009f", new ElmahIoOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                UserAgent = "LoggerVisualizer",
            });
            elmahIoClient.Messages.OnMessage += (sender, args) => args.Message.Version = version;

            Enabled = true;
        }

        public static void TrackException(Exception ex)
        {
#if !DEBUG
            if (Enabled && ex != null && elmahIoClient != null)
            {
                var baseException = ex.GetBaseException();
                var createMessage = new CreateMessage
                {
                    DateTime = DateTime.UtcNow,
                    Detail = ex.ToString(),
                    Type = baseException.GetType().FullName,
                    Title = baseException.Message ?? "An error occurred",
                    Data = PropertiesToData(ex),
                    Severity = "Error",
                    Source = baseException.Source,
                    User = WindowsIdentity.GetCurrent().Name,
                    Hostname = Hostname(),
                    Application = "Exception Visualizer",
                    ServerVariables = new List<Item>
                    {
                        new Item("User-Agent", $"X-ELMAHIO-APPLICATION; OS=Windows; OSVERSION={Environment.OSVersion.Version}; ENGINE=VisualStudio"),
                    }
                };

                elmahIoClient.Messages.CreateAndNotify(new Guid("934272cb-2133-4fa7-b2b6-3113cb0b272b"), createMessage);
            }
#endif
        }

        private static List<Item> PropertiesToData(Exception exception)
        {
            var items = new List<Item>();

            if (exception != null)
            {
                items.AddRange(exception.ToDataList());
            }

            if (SystemParameters.PrimaryScreenWidth > 0) items.Add(new Item("Screen-Width", ((int)SystemParameters.PrimaryScreenWidth).ToString()));
            if (SystemParameters.PrimaryScreenWidth > 0) items.Add(new Item("Screen-Height", ((int)SystemParameters.PrimaryScreenHeight).ToString()));

            return items;
        }

        private static string Hostname()
        {
            var machineName = Environment.MachineName;
            if (!string.IsNullOrWhiteSpace(machineName)) return machineName;

            return Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
    }
}
