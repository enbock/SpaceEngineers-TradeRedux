using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;
using VRage.Game.ModAPI;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
    public class TradeStation : StationBase
    {
        public double ProduceFrom = 0.25f;
        public double ReduceFrom = 0.75f;
        private List<Item> _goods = new List<Item>();

        public override List<Item> Goods
        {
            get { return _goods; }
        }

        public const string StationType = "Elitesuppe_TradeRedux_TradeStation";

        public TradeStation()
        {
        }

        public TradeStation(long ownerId) : base(ownerId, StationType)
        {
            _goods = new List<Item>
            {
                new Item("MyObjectBuilder_Ingot/Platinum", new Price(0.07f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Gold", new Price(0.03f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Uranium", new Price(0.02f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Silver", new Price(0.003f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Magnesium", new Price(0.03f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Silicon", new Price(0.00044f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Nickel", new Price(0.00084f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Ingot/Cobalt", new Price(0.001f), false, true, 100000f, 0),
                //new Item("MyObjectBuilder_Ingot/Iron", new Price(0.000014f), false, true, 100000f, 0),
                new Item("MyObjectBuilder_Component/SpaceCoin", new Price(100f), true, false, 10000f, 0)
            };
        }

        public override void HandleProdCycle()
        {
            IEnumerable<Item> productionItems =
                Goods.Where(good => good.CargoRatio < ProduceFrom || good.CargoRatio > ReduceFrom);

            foreach (Item tradeItem in productionItems)
            {
                double itemCount = 0f;

                if (tradeItem.CargoRatio > ReduceFrom)
                {
                    itemCount = -1f * (tradeItem.CargoSize * 0.01f);
                    if (itemCount > -1f) itemCount = -1f;
                }

                if (tradeItem.CargoRatio < ProduceFrom)
                {
                    itemCount = tradeItem.CargoSize * 0.01f;
                    if (itemCount < 1f) itemCount = 1f;
                }

                itemCount = Math.Round(itemCount, 0);

                double newCargo = itemCount + tradeItem.CurrentCargo;

                if (newCargo > tradeItem.CargoSize) newCargo = tradeItem.CargoSize;
                if (newCargo < 0f) newCargo = 0f;

                tradeItem.CurrentCargo = newCargo;
            }
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            TradeStation loadedData = (TradeStation) oldStationData;
            List<Item> currentGoods = _goods;
            _goods = loadedData._goods;
            
            foreach (Item nowItem in Goods)
            {
                foreach (Item beforeItem in currentGoods)
                {
                    if (nowItem.SerializedDefinition != beforeItem.SerializedDefinition) continue;

                    nowItem.CurrentCargo = beforeItem.CurrentCargo;

                    break; // first out
                }
            }

            ProduceFrom = loadedData.ProduceFrom;
            ReduceFrom = loadedData.ReduceFrom;
        }
    }
}