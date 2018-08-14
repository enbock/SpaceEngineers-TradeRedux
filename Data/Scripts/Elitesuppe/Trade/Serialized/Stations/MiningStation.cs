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
    public class MiningStation : StationBase
    {
        public double credits = 0;
        public double ProduceFrom = 1;
        public double ReduceFrom = 1;
        public const string StationType = "MiningStation";
        
        public MiningStation() { }

        public MiningStation(long ownerId) : base(ownerId, StationType)
        {

            // TradeItem(Name, (price,minPercent,maxPercent,willProduce), sell, buy, cargoSize, 0)
            // buy

            // Setze Uranium ingots und Eis als erz voraus um zu arbeiten...
            Goods.Add(new TradeItem("MyObjectBuilder_Ingot/Uranium", new PriceModel(32f), false, true, 100000, 50));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Ice", new PriceModel(12f), false, true, 250000, 50));

            // sell

            // @todo: Für Beispiel erstmal alle später limitieren..
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Iron", new PriceModel(100f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Nickel", new PriceModel(100f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Cobalt", new PriceModel(100f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Gold", new PriceModel(200f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Silver", new PriceModel(200f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Silicon", new PriceModel(200f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Magnesium", new PriceModel(200f, true), true, false, 10000, 0));
            Goods.Add(new TradeItem("MyObjectBuilder_Ore/Platinum", new PriceModel(300f, true), true, false, 10000, 0));
        }

        public override void HandleProdCycle()
        {
            
            // @Todo: Nochmal überarbeiten .. Es müssen alle "buy" ressourcen auf lager sein .. danach darf erst die "sell" ressourcen Produziert werden .. 
            // hier fehlt noch eine überarbeitung den was ist wenn es im minus rutscht der güter ? aktuell wird es danach auf 0 gesetzt. Sowie die Industrie stellt die arbeit ein.

            // Listen als IEnumerable
            IEnumerable<TradeItem> buyItems     = Goods.Where(good => good.IsBuy && good.CurrentCargo > 0) ;
            IEnumerable<TradeItem> sellItems    = Goods.Where(good => good.IsSell && (good.CargoSize > good.CurrentCargo));
            
            // Listen nochmal als "List" für Count ..
            List<TradeItem> listBuy = Goods.FindAll(good => good.IsBuy && good.CurrentCargo > 0);
            if(listBuy.Count != 2)
            {
                MyAPIGateway.Utilities.ShowMessage(StationType,"disable work: input low");
                return;
            }

            List<TradeItem> listSell = Goods.FindAll(good => good.IsSell && (good.CargoSize > good.CurrentCargo));
            if (listSell.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage(StationType, "disable work: output full");
                return;
            }

            int multi = 0;
            double dif = 0;

            MyAPIGateway.Utilities.ShowMessage(StationType, "update prod");
            // prod from sellItems
            foreach (TradeItem tradeItem in sellItems)
            {
                if (tradeItem.CurrentCargo < tradeItem.CargoSize)
                {
                    dif = tradeItem.CargoSize - tradeItem.CurrentCargo;

                    if(dif > 1)
                    {
                        tradeItem.CurrentCargo+=500;
                    } else
                    {
                        tradeItem.CurrentCargo = tradeItem.CargoSize;
                    }
                    multi++;
                }
            }


            MyAPIGateway.Utilities.ShowMessage(StationType, "update items :"+ multi.ToString());
            // remove prod from buyItems
            foreach (TradeItem tradeItem in buyItems)
            {
                if(tradeItem.CurrentCargo > 0)
                {
                    tradeItem.CurrentCargo -=  multi;
                }

                if (tradeItem.CurrentCargo < 0)
                {
                    tradeItem.CurrentCargo = 0;
                }

                // MyAPIGateway.Utilities.ShowMessage("tradeItem", tradeItem.Definition.ToString()+ " CurrentCargo:" + tradeItem.CurrentCargo.ToString());
            }

        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            base.TakeSettingData(oldStationData);
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
