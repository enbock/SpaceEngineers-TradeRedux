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
        public double ProduceFrom = 0.25f;
        public double ReduceFrom = 0.75f;
        public const string StationType = "TradeStation";
        
        public TradeStation() { }

        public TradeStation(long ownerId) : base(ownerId, StationType)
        {
            Goods.Add(new Item("MyObjectBuilder_Ingot/Gold", new Price(2f), true, true, 1000, 0));
            Goods.Add(new Item("MyObjectBuilder_Ingot/Silver", new Price(1f), true, true, 1000, 0));
        }

        public override void HandleProdCycle()
        {
            IEnumerable<Item> productionItems = Goods.Where(good => good.CargoRatio < ProduceFrom || good.CargoRatio > ReduceFrom);

            foreach (Item tradeItem in productionItems)
            {
                bool sell = false;
                MyDefinitionId itemId = tradeItem.Definition;
                double itemCount = 0f;

                if (tradeItem.CargoRatio > ReduceFrom)
                {
                    itemCount = -1f * (tradeItem.CargoSize * 0.01f);
                }

                if (tradeItem.CargoRatio < ProduceFrom)
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
                        var sellPrice = tradeItem.Price.GetSellPrice(tradeItem.CargoRatio);
                    }
                    else
                    {
                        tradeItem.CurrentCargo += itemCount;
                        var buyPrice = tradeItem.Price.GetBuyPrice(tradeItem.CargoRatio);
                    }
                }
                MyAPIGateway.Utilities.ShowMessage("HandleProdCycle", tradeItem.Definition + "s: " + itemCount.ToString("0.#####") + "/" + tradeItem.CargoRatio.ToString("0.###") + "/" + tradeItem.CurrentCargo);
            }
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            base.TakeSettingData(oldStationData);
            foreach (Item beforeItem in oldStationData.Goods)
            {
                foreach (Item nowItem in Goods)
                {
                    if (nowItem.SerializedDefinition != beforeItem.SerializedDefinition) continue;

                    nowItem.Price.Amount = beforeItem.Price.Amount;
                    nowItem.Price.MinPercent = beforeItem.Price.MinPercent;
                    nowItem.Price.MaxPercent = beforeItem.Price.MaxPercent;
                    nowItem.CargoSize = beforeItem.CargoSize;

                    break; // first out
                }
            }
        }
    }
}
