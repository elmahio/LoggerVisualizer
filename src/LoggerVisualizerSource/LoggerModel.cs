using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LoggerVisualizerSource
{
    [DataContract]
    public class LoggerModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Enabled { get; set; }

        [DataMember]
        public string MinLevel { get; set; }

        [DataMember]
        public List<Logger> Loggers { get; set; } = [];
    }

    [DataContract]
    public class Logger
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ExternalScope { get; set; }

        [DataMember]
        public string MinLevel { get; set;  }

        #region Console
        [DataMember]
        public string FormatterName { get; set; }

        [DataMember]
        public string DisableColors { get; set; }

        [DataMember]
        public string LogToStandardErrorThreshold { get; set; }

        [DataMember]
        public string MaxQueueLength { get; set; }

        [DataMember]
        public string QueueFullMode { get; set; }

        [DataMember]
        public string TimestampFormat { get; set; }

        [DataMember]
        public string UseUtcTimestamp { get; set; }
        #endregion

        #region EventLog
        [DataMember]
        public string MachineName { get; set; }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public string LogName { get; set; }
        #endregion

        #region ElmahIo
        [DataMember]
        public string ApiKey { get; set; }

        [DataMember]
        public string LogId { get; set; }
        #endregion
    }
}
