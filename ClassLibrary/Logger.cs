using Serilog;

namespace ClassLibrary
{
    public static class Logger
    {

        public static ILogger Logger_;
        public static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($@"C:\ProgramData\Autodesk\Revit\Addins\{ClassLibrary.GlobalData.VersionRevit}\AcousticConstructor/logs/log.txt").MinimumLevel.Debug()
                .CreateLogger();

            Logger_ = Log.Logger;

        }
    }
}
