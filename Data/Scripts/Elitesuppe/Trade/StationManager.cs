using System.Collections.Generic;
using System.Linq;
using EliteSuppe.Trade.Stations;
using Sandbox.ModAPI;

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
            return _stationList.Where(lcd => lcd != null && lcd.Station != null).Select(lcd =>
                new StationWithTradeBlock {Station = lcd.Station, TradeBlock = lcd.LcdPanel, Lcd = lcd});
        }

        private static void CleanUpStationList()
        {
            var cleanStations = _stationList
                .Where(lcd => lcd != null && lcd.LcdPanel != null && !lcd.MarkedForClose && !lcd.Closed).ToList();
            _stationList = cleanStations;
        }
    }

    public class StationWithTradeBlock
    {
        public StationBase Station { get; set; }
        public IMyTextPanel TradeBlock { get; set; }
        public TradeLogicComponent Lcd { get; set; }
    }
}