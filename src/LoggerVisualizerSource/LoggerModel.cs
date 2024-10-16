using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LoggerVisualizerSource
{
    [DataContract]
    public class LoggerModel
    {
        [DataMember]
        public string Name { get; set; } = "N/A";

        [DataMember]
        public string Enabled { get; set; } = "N/A";

        [DataMember]
        public string MinLevel { get; set; } = "N/A";

        [DataMember]
        public List<Logger> Loggers { get; set; } = [];
    }

    [DataContract]
    public class Logger
    {
        [DataMember]
        public string Name { get; set; } = "N/A";

        [DataMember]
        public string ExternalScope { get; set; } = "N/A";

        [DataMember]
        public string MinLevel { get; set; } = "N/A";

        #region Console
        [DataMember]
        public string FormatterName { get; set; } = "N/A";

        [DataMember]
        public string DisableColors { get; set; } = "N/A";

        [DataMember]
        public string LogToStandardErrorThreshold { get; set; } = "N/A";

        [DataMember]
        public string MaxQueueLength { get; set; } = "N/A";

        [DataMember]
        public string QueueFullMode { get; set; } = "N/A";

        [DataMember]
        public string TimestampFormat { get; set; } = "N/A";

        [DataMember]
        public string UseUtcTimestamp { get; set; } = "N/A";
        #endregion

        #region EventLog
        [DataMember]
        public string MachineName { get; set; } = "N/A";

        [DataMember]
        public string SourceName { get; set; } = "N/A";

        [DataMember]
        public string LogName { get; set; } = "N/A";
        #endregion

        #region ElmahIo
        [DataMember]
        public string ApiKey { get; set; } = "N/A";

        [DataMember]
        public string LogId { get; set; } = "N/A";
        #endregion
    }
}
