using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ClassLibrary
{
    public static class Logger
    {
        public static ILogger CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor/logs/log.txt").MinimumLevel.Debug()
                .CreateLogger();

            return Log.Logger;

        }
    }
}
