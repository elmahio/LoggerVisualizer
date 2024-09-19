using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace LoggerVisualizer
{
    [VisualStudioContribution]
    public class ExtensionEntrypoint : Extension
    {
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                id: "LoggerVisualizer",
                version: this.ExtensionAssemblyVersion,
                publisherName: "elmahio",
                displayName: "ILogger Debugger Visualizer Extension",
                description: "Logger Debug Visualizer for Visual Studio.")
            {
                Tags = ["visualizer", "logging", "logger", "ilogger", "debug", "debugging"],
                Icon = "icon.png",
            }
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            Telemetry.Initialize(ExtensionAssemblyVersion?.ToString());
            try
            {
                base.InitializeServices(serviceCollection);
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
                throw;
            }

            // You can configure dependency injection here by adding services to the serviceCollection.
        }
    }
}
