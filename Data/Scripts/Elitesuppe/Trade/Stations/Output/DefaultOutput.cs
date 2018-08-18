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

        public virtual void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid)
        {
            if (grid == null) throw new ArgumentException();

            var builder = new StringBuilder();
            builder.AppendLine("StationName: " + grid.CustomName);

            output.Add("station", builder);
        }

        protected static StringBuilder CloneOutput(StringBuilder input)
        {
            StringBuilder output = new StringBuilder();
            int index;
            string[] lines = input.ToString().Split('\n');

            for (index = 0; index < lines.Length - 1; index++)
            {
                output.AppendLine(lines[index].TrimEnd('\r'));
            }

            return output;
        }
    }
}