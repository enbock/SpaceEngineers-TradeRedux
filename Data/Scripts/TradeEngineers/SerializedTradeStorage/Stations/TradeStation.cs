using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeEngineers.Inventory;
using VRage.Game;
using TradeEngineers.TradeGoods;
using Sandbox.ModAPI;

namespace TradeEngineers.SerializedTradeStorage
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.DataFormat)]
    public class TradeStation : StationBase
    {
        public new static string StationType = "TradeStation";
        public TradeStation() { }

        public TradeStation(bool init, long ownerId, string type) : base(ownerId, type)
        {
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Gold", new PriceModel(2f, true, 0.6f, 1.4f), true, true, 1000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Silver", new PriceModel(1f, true, 0.6f, 1.4f), true, true, 1000, 0));
        }

        public override void HandleProdCycle(double fullprodtime)
        {
            double ProduceFrom = 0.25f;
            double RecudeFrom = 0.75f;

            IEnumerable<TradeItem> proditems = Goods.Where(good => good.CargoRatio < ProduceFrom || good.CargoRatio > RecudeFrom);

            foreach (TradeItem tradeitem in proditems)
            {
                bool sell = false;
                MyDefinitionId itemid = tradeitem.Definition;
                double itemCount = 0f;

                if (tradeitem.CargoRatio > RecudeFrom)
                {
                    itemCount = -1f * (tradeitem.CargoSize * 0.01f);
                }

                if (tradeitem.CargoRatio < ProduceFrom)
                {
                    itemCount = tradeitem.CargoSize * 0.01f;
                }

                if (itemCount < 0)
                {
                    itemCount = Math.Abs(itemCount);
                    sell = true;
                }

                if (itemCount > tradeitem.CargoSize) itemCount = tradeitem.CargoSize;

                if (!(ItemDefinitionFactory.Ores.Contains(itemid) || ItemDefinitionFactory.Ingots.Contains(itemid)))
                {
                    itemCount = Math.Floor(itemCount);
                }

                if (itemCount > 0)
                {
                    if (sell)
                    {
                        tradeitem.CurrentCargo -= itemCount;
                        var actsellprice = tradeitem.PriceModel.GerSellPrice(tradeitem.CargoRatio);
                    }
                    else
                    {
                        tradeitem.CurrentCargo += itemCount;
                        var actbuyprice = tradeitem.PriceModel.GetBuyPrice(tradeitem.CargoRatio);
                    }
                }
                MyAPIGateway.Utilities.ShowMessage("HandleProdCycle", tradeitem.Definition.ToString() + "s: " + itemCount.ToString("0.#####") + "/" + tradeitem.CargoRatio.ToString("0.###") + "/" + tradeitem.CurrentCargo);
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
