using System;
using System.Collections.Generic;
using System.Text;
using Elitesuppe.Trade;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations.Output
{
    public class DefaultOutput : IOutputRepresentor
    {
        public StationBase Station { get; set; }

        public DefaultOutput(StationBase station)
        {
            Station = station;
        }

        public void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid)
        {
            if (grid == null) throw new ArgumentException();
            
            var builder = new StringBuilder();
            builder.AppendLine("StationName:" + grid.CustomName);
            builder.AppendLine("StationType:" + Station.Type);
            
            output.Add("station", builder);
        }
    }
}