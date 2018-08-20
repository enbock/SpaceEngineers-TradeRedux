using System.Collections.Generic;
using System.Linq;
using System.Text;
using EliteSuppe.Trade.Items;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations.Output
{
    public class TradeStationOutput : DefaultOutput
    {
        public TradeStationOutput(StationBase station) : base(station)
        {
        }

        public override void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid)
        {
            base.CreateOutput(output, grid);

            StringBuilder baseOutput = output["station"];
            StringBuilder buyBuilder = CloneOutput(baseOutput);
            StringBuilder sellBuilder = CloneOutput(baseOutput);

            buyBuilder.AppendLine("Purchasing:");
            sellBuilder.AppendLine("Selling:");

            List<Item> stationGoods = Station?.Goods;
            if (stationGoods == null)
            {
                MyAPIGateway.Utilities.ShowMessage("TR-Log", "NO GOODS");
                return;
            }

            foreach (var tradeItem in stationGoods)
            {

                if (tradeItem.IsSell)
                {
                    double sellPrice = tradeItem.Price.GetSellPrice(tradeItem.CargoRatio);
                    sellBuilder.AppendLine(FormatItem(tradeItem, sellPrice));
                }
                else if (tradeItem.IsBuy)
                {
                    double buyPrice = tradeItem.Price.GetBuyPrice(tradeItem.CargoRatio);
                    buyBuilder.AppendLine(FormatItem(tradeItem, buyPrice));
                }
            }

            output.Add("buy", buyBuilder);
            output.Add("purchase", buyBuilder);
            output.Add("sell", sellBuilder);
        }

    }
}