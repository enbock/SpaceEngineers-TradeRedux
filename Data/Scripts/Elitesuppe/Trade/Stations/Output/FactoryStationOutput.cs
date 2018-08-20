using System.Collections.Generic;
using System.Text;
using EliteSuppe.Trade.Items;
using Sandbox.ModAPI;
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

            StringBuilder baseOutput = output["station"];
            StringBuilder outputBuilder = CloneOutput(baseOutput);
            StringBuilder purchaseBuilder = output["purchase"];
            StringBuilder sellBuilder = output["sell"];
            StringBuilder bufferBuilder = CloneOutput(baseOutput);

            outputBuilder.AppendLine("Purchasing:");
            foreach (Item item in station.ResourceStock)
            {
                string line = FormatItem(item, item.Price.GetBuyPrice(item.CargoRatio));
                outputBuilder.AppendLine(line);
                purchaseBuilder.AppendLine(line);
            }
            
            outputBuilder.AppendLine();
            outputBuilder.AppendLine("Selling:");
            foreach (Item item in station.ProductStock)
            {
                string line = FormatItem(item, item.Price.GetBuyPrice(item.CargoRatio));
                outputBuilder.AppendLine(line);
                sellBuilder.AppendLine(line);
            }
            
            outputBuilder.AppendLine();
            outputBuilder.AppendLine("Purchasing and selling:");
            bufferBuilder.AppendLine("Purchasing and selling:");
            foreach (Item item in station.BufferStock)
            {
                string value = FormatItem(item, item.Price.GetBuyPrice(item.CargoRatio));
                outputBuilder.AppendLine(value);
                bufferBuilder.AppendLine(value);
            }

            output.Add("factory", outputBuilder);
            output.Add("buffer", bufferBuilder);
        }
    }
}