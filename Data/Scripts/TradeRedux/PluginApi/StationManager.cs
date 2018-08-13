using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using TradeRedux.SerializedTradeStorage;

namespace TradeRedux.PluginApi
{
    public static class StationManager
    {
        private static List<TradeBlock> stationList = new List<TradeBlock>();

        public static void Register(TradeBlock block)
        {            
            if (!stationList.Contains(block))
                stationList.Add(block);
        }

        public static IEnumerable<StationWithTradeBlock> GetStations()
        {
            CleanUpStationList();
            return stationList.Where(lcd => lcd != null && lcd.Station != null).Select(lcd => new StationWithTradeBlock { Station = lcd.Station, TradeBlock = lcd.LcdPanel, LCD = lcd });
        }

        private static void CleanUpStationList()
        {
            var cleanStations = stationList.Where(lcd => lcd != null && lcd.LcdPanel!=null && !lcd.MarkedForClose && !lcd.Closed).ToList();
            stationList = cleanStations;
        }
    }

    public class StationWithTradeBlock
    {
        public StationBase Station { get; set; }
        public Sandbox.ModAPI.IMyTextPanel TradeBlock { get; set; }
        public TradeBlock LCD { get; set; }
    }
}
