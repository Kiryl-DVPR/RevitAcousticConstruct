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
        public static ILogger Logger1 { get; private set; }

        public static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor/logs/log.txt").MinimumLevel.Debug()
                .CreateLogger();

            Logger1 = Log.Logger;

        }
    }
}
