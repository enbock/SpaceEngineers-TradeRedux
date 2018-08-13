using System;
using Sandbox.ModAPI;

namespace TradeRedux.PluginApi
{
    public class Logger
    {
        private static System.IO.TextWriter Writer = null;

        public Logger()
        {
        }

        public static string Log(string text)
        {
            String now = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            MyAPIGateway.Utilities.ShowMessage("TE-Log", text);
            if (Writer == null)
            {
                string fileName = "TradeBlock.log";
                try
                {
                    Writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, typeof(TradeBlock));
                }
                catch (Exception)
                {
                    MyAPIGateway.Utilities.ShowMessage("TradeEngineers IO", "Could not open the log file:" + fileName);
                }
            }
            string line = now + ": " + text;

            if (Writer != null)
            {
                Writer.WriteLine(line);
                Writer.Flush();
            }

            return line;
        }

        public static void Close()
        {
            if (Writer != null)
            {
                Writer.Close();
                Writer = null;
            }
        }
    }
}