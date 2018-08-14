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
    public class IronForge : StationBase
    {
        public double credits = 0;
        public double ProduceFrom = 1;
        public double ReduceFrom = 1;
        public const string StationType = "IronForge";
        
        public IronForge() { }

        public IronForge(long ownerId) : base(ownerId, StationType)
        {

            // Item(Name, (price,minPercent,maxPercent,willProduce), sell, buy, cargoSize, 0)
            // buy

            // Setze Uranium ingots und Eis als erz voraus um zu arbeiten...
            Goods.Add(new Item("MyObjectBuilder_Ingot/Uranium", new Price(32f), false, true, 25000, 50));
            Goods.Add(new Item("MyObjectBuilder_Ore/Ice", new Price(12f), false, true, 25000, 50));
            Goods.Add(new Item("MyObjectBuilder_Ingot/Iron", new Price(12f), false, true, 25000, 50));

            // sell
            // @todo: Für Beispiel erstmal alle später limitieren..
            Goods.Add(new Item("MyObjectBuilder_Component/SteelPlate", new Price(100f), true, false, 100000, 0));
            Goods.Add(new Item("MyObjectBuilder_Component/Construction Component", new Price(100f), true, false, 100000, 0));
            Goods.Add(new Item("MyObjectBuilder_Component/Interior Plate", new Price(100f), true, false, 100000, 0));
        }

        public override void HandleProdCycle()
        {
            
            // @Todo: Nochmal überarbeiten .. Es müssen alle "buy" ressourcen auf lager sein .. danach darf erst die "sell" ressourcen Produziert werden .. 
            // hier fehlt noch eine überarbeitung den was ist wenn es im minus rutscht der güter ? aktuell wird es danach auf 0 gesetzt. Sowie die Industrie stellt die arbeit ein.

            // Listen als IEnumerable
            IEnumerable<Item> buyItems     = Goods.Where(good => good.IsBuy && good.CurrentCargo > 0) ;
            IEnumerable<Item> sellItems    = Goods.Where(good => good.IsSell && (good.CargoSize > good.CurrentCargo));
            
            // Listen nochmal als "List" für Count ..
            List<Item> listBuy = Goods.FindAll(good => good.IsBuy && good.CurrentCargo > 0);
            if(listBuy.Count != 2)
            {
                MyAPIGateway.Utilities.ShowMessage(StationType,"disable work: input low");
                return;
            }

            List<Item> listSell = Goods.FindAll(good => good.IsSell && (good.CargoSize > good.CurrentCargo));
            if (listSell.Count == 0)
            {
                MyAPIGateway.Utilities.ShowMessage(StationType, "disable work: output full");
                return;
            }

            int multi = 0;
            double dif = 0;

            MyAPIGateway.Utilities.ShowMessage(StationType, "update prod");
            // prod from sellItems
            foreach (Item tradeItem in sellItems)
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


            MyAPIGateway.Utilities.ShowMessage(StationType, "update items :"+ multi);
            // remove prod from buyItems
            foreach (Item tradeItem in buyItems)
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
