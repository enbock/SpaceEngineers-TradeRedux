using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using EliteSuppe.Trade.Items;

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

        public TradeStation() : base(StationType)
        {
        }

        public TradeStation(bool init = false) : base(StationType)
        {
            if (!init) return; // deserialization created
            
            _goods = new List<Item>
            {
                new Item("Ingot/Platinum", new Price(0.07f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Gold", new Price(0.03f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Uranium", new Price(0.02f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Silver", new Price(0.003f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Magnesium", new Price(0.03f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Silicon", new Price(0.00044f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Nickel", new Price(0.00084f, 0.5f, 1.5f), new Price(), 100000f, 0f),
                new Item("Ingot/Cobalt", new Price(0.001f, 0.5f, 1.5f), new Price(), 100000f, 0),
                new Item("Component/SpaceCoin", new Price(0f), new Price(100f), 10000f, 0f)
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

                itemCount = Math.Round(itemCount);

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