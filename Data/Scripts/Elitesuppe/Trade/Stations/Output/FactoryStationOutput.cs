using System.Collections.Generic;
using System.Linq;
using System.Text;
using EliteSuppe.Trade.Items;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations.Output
{
    public class FactoryStationOutput : TradeStationOutput
    {
        public FactoryStationOutput(StationBase station) : base(station)
        {
        }

        public override void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid)
        {
            base.CreateOutput(output, grid);

            FactoryStation station = Station as FactoryStation;
            if (station == null) return;

            StringBuilder outputBuilder = CloneOutput(output["station"]);
            StringBuilder progressBuilder = CloneOutput(output["station"]);

            outputBuilder.AppendLine("Purchasing:");
            foreach (Item item in station.Stock.Where(i => i.IsPurchasing))
            {
                string line = FormatItem(item, item.PurchasePrice.GetStockPrice(item.CargoRatio));
                outputBuilder.AppendLine(line);
            }

            outputBuilder.AppendLine();
            outputBuilder.AppendLine("Selling:");
            foreach (Item item in station.Stock.Where(i => i.IsSelling))
            {
                string line = FormatItem(item, item.SellPrice.GetStockPrice(item.CargoRatio));
                outputBuilder.AppendLine(line);
            }


            outputBuilder.AppendLine();
            outputBuilder.AppendLine("Progress:");
            progressBuilder.AppendLine("Progress:");
            foreach (KeyValuePair<string, double> pair in station.Progress())
            {
                string line = $"{pair.Key}: {pair.Value:0.##}%";
                outputBuilder.AppendLine(line);
                progressBuilder.AppendLine(line);
            }

            output.Add("factory", outputBuilder);
            output.Add("progress", progressBuilder);
        }
    }
}