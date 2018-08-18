using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations.Output
{
    public class TradeStationOutput : DefaultOutput
    {
        private new TradeStation Station
        {
            get { return base.Station as TradeStation; }
        }

        public TradeStationOutput(StationBase station) : base(station)
        {
        }

        public override void CreateOutput(Dictionary<string, StringBuilder> output, IMyCubeGrid grid)
        {
            base.CreateOutput(output, grid);

            StringBuilder baseOutput = output["station"];
            StringBuilder buyBuilder = CloneOutput(baseOutput);
            StringBuilder sellBuilder = CloneOutput(baseOutput);

            buyBuilder.AppendLine("Buying:");
            sellBuilder.AppendLine("Selling:");

            foreach (var tradeItem in Station.Goods)
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
            output.Add("sell", sellBuilder);
        }

        private static string FormatItem(Item tradeItem, double price)
        {
            double perItem = 1f;

            if (price < 1f)
            {
                perItem = Math.Floor(1f / price);
                price = 1f;
            }

            string formattedPerItem = (Math.Abs(perItem - 1f) > 0 ? $"per {perItem:0.#} " : "");
            double stock = tradeItem.CargoRatio * 100;

            return $"{tradeItem}: {price:0.##}{Definitions.CreditSymbol} {formattedPerItem}(Stock: {stock:0.#}%)";
        }
    }
}