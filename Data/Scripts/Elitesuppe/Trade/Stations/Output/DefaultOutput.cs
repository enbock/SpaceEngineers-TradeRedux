using System;
using System.Collections.Generic;
using System.Text;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;
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
            builder.AppendLine("Type: " + Station.GetType());
            builder.AppendLine();

            output.Add("station", builder);
        }

        protected static StringBuilder CloneOutput(StringBuilder input, StringBuilder output = null)
        {
            if(output == null) output = new StringBuilder();
            int index;
            string[] lines = input.ToString().Split('\n');

            for (index = 0; index < lines.Length - 1; index++)
            {
                output.AppendLine(lines[index].TrimEnd('\r'));
            }
            
            return output;
        }
        
        protected static string FormatItem(Item item, double price)
        {
            double perItem = 1f;

            if (price < 1f)
            {
                perItem = Math.Floor(1f / price);
                price = 1f;
            }

            string formattedPerItem = Math.Abs(perItem - 1f) > 0 ? $"per {perItem:0.#} " : "";
            double stock = item.CargoRatio * 100;

            return $"{item}: {price:0.##}{Definitions.CreditSymbol} {formattedPerItem}(Stock: {stock:0.#}%)";
        }
    }
}