using System;
using Sandbox.ModAPI;

namespace TradeEngineers.PluginApi
{
    public class Logger
    {
        private static System.IO.TextWriter logger = null;

        public Logger()
        {
        }

        public static void Log(string text)
        {
            MyAPIGateway.Utilities.ShowMessage("TE-Log", text);
            if (logger == null)
            {
                string fileName = "TradeBlock.log";
                try
                {
                    logger = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, typeof(TradeBlock));
                }
                catch (Exception)
                {
                    MyAPIGateway.Utilities.ShowMessage("TradeEngineers IO", "Could not open the log file:" + fileName);
                    return;
                }
            }

            String now = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            logger.WriteLine(now + ": " + text);
            logger.Flush();
        }

        public static void Close()
        {
            if (logger != null)
                logger.Close();
        }
    }
}