using Elitesuppe.Trade;

namespace EliteSuppe.Trade.Stations.Output
{
    public static class StationOutputFactory
    {
        public static IOutputRepresentor CreateRepresentor(StationBase station)
        {
            IOutputRepresentor outputRepresentor;

            if (station is TradeStation) outputRepresentor = new TradeStationOutput(station);
            else outputRepresentor = new DefaultOutput(station);

            return outputRepresentor;
        }
    }
}