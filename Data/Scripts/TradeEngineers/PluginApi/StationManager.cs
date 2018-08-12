using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using TradeEngineers.SerializedTradeStorage;

namespace TradeEngineers.PluginApi
{
    public static class StationManager
    {
        private static List<LCDBlock> _knownTEBlocks = new List<LCDBlock>();

        public static void Register(LCDBlock block)
        {            
            if (!_knownTEBlocks.Contains(block))
                _knownTEBlocks.Add(block);
        }

        public static IEnumerable<StationWithTradeBlock> GetStations()
        {
            CleanBlocks();
            return _knownTEBlocks.Where(lcd => lcd != null && lcd.Station != null).Select(lcd => new StationWithTradeBlock { Station = lcd.Station, TradeBlock = lcd.myLcd, LCD = lcd });
        }

        private static void CleanBlocks()
        {
            var cleanStations = _knownTEBlocks.Where(lcd => lcd != null && lcd.myLcd!=null && !lcd.MarkedForClose && !lcd.Closed).ToList();
            _knownTEBlocks = cleanStations;
        }
    }

    public class StationWithTradeBlock
    {
        public StationBase Station { get; set; }
        public Sandbox.ModAPI.IMyTextPanel TradeBlock { get; set; }
        public LCDBlock LCD { get; set; }
    }
}
