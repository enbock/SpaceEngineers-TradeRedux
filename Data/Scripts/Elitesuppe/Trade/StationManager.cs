
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;

using Elitesuppe.Trade.Serialized;
using Elitesuppe.Trade.Serialized.Stations;
using Elitesuppe;

namespace Elitesuppe.Trade
{
    public static class StationManager
    {
        private static List<TradeLogicComponent> _stationList = new List<TradeLogicComponent>();

        public static void Register(TradeLogicComponent block)
        {            
            if (!_stationList.Contains(block))
                _stationList.Add(block);
        }

        public static IEnumerable<StationWithTradeBlock> GetStations()
        {
            CleanUpStationList();
            return _stationList.Where(lcd => lcd?.Station != null).Select(lcd => new StationWithTradeBlock { Station = lcd.Station, TradeBlock = lcd.LcdPanel, LCD = lcd });
        }

        private static void CleanUpStationList()
        {
            var cleanStations = _stationList.Where(lcd => lcd?.LcdPanel != null && !lcd.MarkedForClose && !lcd.Closed).ToList();
            _stationList = cleanStations;
        }
    }

    public class StationWithTradeBlock
    {
        public StationBase Station { get; set; }
        public Sandbox.ModAPI.IMyTextPanel TradeBlock { get; set; }
        public TradeLogicComponent LCD { get; set; }
    }
}
