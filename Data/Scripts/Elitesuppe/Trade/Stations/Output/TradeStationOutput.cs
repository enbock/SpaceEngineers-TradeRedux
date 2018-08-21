using System.Collections.Generic;
using System.Text;
using EliteSuppe.Trade.Items;
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
            StringBuilder purchaseBuilder = CloneOutput(baseOutput);
            StringBuilder sellBuilder = CloneOutput(baseOutput);

            purchaseBuilder.AppendLine("Purchasing:");
            sellBuilder.AppendLine("Selling:");

            List<Item> stationGoods = Station?.Goods;
            if (stationGoods == null) return;

            foreach (var tradeItem in stationGoods)
            {
                if (tradeItem.IsSelling)
                {
                    double sellPrice = tradeItem.SellPrice.GetStockPrice(tradeItem.CargoRatio);
                    sellBuilder.AppendLine(FormatItem(tradeItem, sellPrice));
                }
                else if (tradeItem.IsPurchasing)
                {
                    double buyPrice = tradeItem.PurchasePrice.GetStockPrice(tradeItem.CargoRatio);
                    purchaseBuilder.AppendLine(FormatItem(tradeItem, buyPrice));
                }
            }

            output.Add("purchase", purchaseBuilder);
            output.Add("sell", sellBuilder);
        }

    }
}