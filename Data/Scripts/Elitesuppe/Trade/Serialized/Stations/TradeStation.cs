using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using Sandbox.ModAPI;

using Elitesuppe.Trade.Inventory;
using Elitesuppe.Trade.TradeGoods;
using Elitesuppe.Trade.Serialized.Items;

namespace Elitesuppe.Trade.Serialized.Stations
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.Version)]
    public class TradeStation : StationBase
    {
        public const string StationType = "TradeStation";
        public TradeStation() { }

        public TradeStation(bool init, long ownerId) : base(ownerId, StationType)
        {
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Gold", new PriceModel(2f, true, 0.6f, 1.4f), true, true, 1000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Silver", new PriceModel(1f, true, 0.6f, 1.4f), true, true, 1000, 0));
        }

        public override void HandleProdCycle()
        {
            const double produceFrom = 0.25f;
            const double recedeFrom = 0.75f;

            IEnumerable<TradeItem> productionItems = Goods.Where(good => good.CargoRatio < produceFrom || good.CargoRatio > recedeFrom);

            foreach (TradeItem tradeItem in productionItems)
            {
                bool sell = false;
                MyDefinitionId itemId = tradeItem.Definition;
                double itemCount = 0f;

                if (tradeItem.CargoRatio > recedeFrom)
                {
                    itemCount = -1f * (tradeItem.CargoSize * 0.01f);
                }

                if (tradeItem.CargoRatio < produceFrom)
                {
                    itemCount = tradeItem.CargoSize * 0.01f;
                }

                if (itemCount < 0)
                {
                    itemCount = Math.Abs(itemCount);
                    sell = true;
                }

                if (itemCount > tradeItem.CargoSize) itemCount = tradeItem.CargoSize;

                if (!(ItemDefinitionFactory.Ores.Contains(itemId) || ItemDefinitionFactory.Ingots.Contains(itemId)))
                {
                    itemCount = Math.Floor(itemCount);
                }

                if (itemCount > 0)
                {
                    if (sell)
                    {
                        tradeItem.CurrentCargo -= itemCount;
                        var sellPrice = tradeItem.PriceModel.GetSellPrice(tradeItem.CargoRatio);
                    }
                    else
                    {
                        tradeItem.CurrentCargo += itemCount;
                        var buyPrice = tradeItem.PriceModel.GetBuyPrice(tradeItem.CargoRatio);
                    }
                }
                MyAPIGateway.Utilities.ShowMessage("HandleProdCycle", tradeItem.Definition + "s: " + itemCount.ToString("0.#####") + "/" + tradeItem.CargoRatio.ToString("0.###") + "/" + tradeItem.CurrentCargo);
            }
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            foreach (TradeItem beforeItem in oldStationData.Goods)
            {
                foreach (TradeItem nowItem in Goods)
                {
                    if (nowItem.SerializedDefinition != beforeItem.SerializedDefinition) continue;

                    nowItem.PriceModel.Price = beforeItem.PriceModel.Price;
                    nowItem.PriceModel.MinPercent = beforeItem.PriceModel.MinPercent;
                    nowItem.PriceModel.MaxPercent = beforeItem.PriceModel.MaxPercent;
                    nowItem.CargoSize = beforeItem.CargoSize;

                    break; // first out
                }
            }
        }
    }
}
