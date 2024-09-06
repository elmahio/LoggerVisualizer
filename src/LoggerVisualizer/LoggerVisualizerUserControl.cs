using LoggerVisualizer.Models;
using LoggerVisualizerSource;
using Microsoft.VisualStudio.Extensibility.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerVisualizer
{
    internal class LoggerVisualizerUserControl : RemoteUserControl
    {
        public LoggerVisualizerUserControl(LoggerRootViewModel model) : base(model)
        {
        }
    }
}
