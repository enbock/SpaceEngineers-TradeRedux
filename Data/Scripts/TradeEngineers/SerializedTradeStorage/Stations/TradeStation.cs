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
        public TradeStation() { }

        public TradeStation(bool init)
        {
            /*var pricelist = PriceFinder.BuildPriceModelList(new List<MyDefinitionId>());

            //Change MinPercent for Components:
            foreach (var itemid in ItemDefinitionFactory.Components)
            {
                pricelist[itemid].MinPercent = 0.4;
                pricelist[itemid].MaxPercent = 1.1;
            }*/
            //Goods.AddRange(ItemDefinitionFactory.Components.Select(i => new TradeItem(i, pricelist[i], true,true)).ToList());
            //Goods.AddRange(ItemDefinitionFactory.Ingots.Select(i => new TradeItem(i, pricelist[i], true, true)));
            //Goods.AddRange(ItemDefinitionFactory.Ores.Select(i => new TradeItem(i, pricelist[i], true, true)));
            //Goods.AddRange(ItemDefinitionFactory.PlayerTools.Select(i => new TradeItem(i, pricelist[i], true, true)));

            //Goods.Add(new TradeItem(new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Gold"), new PriceModel(1f, true, 0.6f, 1.4f), true, true, 100000000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Gold", new PriceModel(1f, true, 0.6f, 1.4f), true, true, 100000000, 0));

            //Goods.AddRange(ItemDefinitionFactory.Ammunitions.Select(i => new TradeItem(i, pricelist[i], true, true, 100, 00)));
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

                    break; // first out
                }
            }
        }

        public override string StationTyp
        {
            get
            {
                return "TradeStation";
            }
        }
    }
}
