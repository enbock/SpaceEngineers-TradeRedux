using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using TradeEngineers.Inventory;
using TradeEngineers.TradeGoods;
using System.Text.RegularExpressions;
using VRage.ModAPI;
using TradeEngineers;
using TradeEngineers.Exceptions;



namespace TradeEngineers.SerializedTradeStorage
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.DataFormat)]
    public abstract class StationBase
    {
        public virtual string StationTyp { get { return "StationBaseType"; } }
        public double BaseCargoSize { get; set; } = 1000;
        public List<TradeItem> Goods { get; } = new List<TradeItem>();

        public StationBase()
        {

        }

        public static StationBase Factory(string blockName, long ownerid)
        {
            if (string.IsNullOrWhiteSpace(blockName)) throw new ArgumentException("Station Block name was empty");

            switch (blockName.ToUpper())
            {
                case "TRADESTATION": return new TradeStation(true);
            }

            throw new ArgumentException("Station Block name did not match a station kind");
        }
        
        public virtual void HandleProdCycle(double fullprodtime)
        {
            /* 
             * Sieht nach produktions routine aus...verschieben wir in entsprechende sub class.
             * 
             * 
            int proditemcnt = 0; //Anzahl der zu produzierenden Güter
            int numberofslots = 0;
            var proditems = Goods.Where(g => g.PriceModel.IsProducent && g.CargoRatio < 1);
            var proditemsrated = proditems.Where(g => g.CargoRatio < PrimProdLvl).ToList();
            if (proditemsrated.Count > 0)
            {
                proditemcnt = proditemsrated.Count;
                proditems = proditemsrated;
            }
            else
                proditemcnt = proditems.ToList().Count();

            numberofslots = proditemcnt; //Erstmal bekommt gedes Item einen Slot zugewiesen
            //MyAPIGateway.Utilities.ShowMessage("ProdInfo:", "Count: " + proditemcnt);

            Dictionary<TradeItem, double> AktiveProduktion = new Dictionary<TradeItem, double>();

            var ordertitems = proditems.OrderBy(g => g.CargoRatio);
            foreach (var tradeitem in ordertitems) /// Vorverarbeitung um Produktionsprozess zu optimieren
            {
                var itemid = tradeitem.Definition;
                var freecargo = tradeitem.CargoSize - tradeitem.CurrentCargo;
                if (freecargo == 0) continue;
                double itemcount = freecargo;

                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);

                foreach (var key in useditemsdict.Keys)
                {
                    var amount = useditemsdict[key];
                    var itemneeded = Goods.Find(g => g.Definition == key);

                    if (itemneeded == null)
                    {
                        //HydrogenBottle fehlt irgendwie!
                        //MyAPIGateway.Utilities.ShowMessage("HandleProdCycle", "Needed item missing!! " + key.ToString());
                        itemcount = 0;
                        break;
                    }
                    if (itemcount > (itemneeded.CurrentCargo / amount)) itemcount = itemneeded.CurrentCargo / amount;
                }
                if (!(ItemDefinitionFactory.Ores.Contains(itemid) || ItemDefinitionFactory.Ingots.Contains(itemid)))
                {
                    itemcount = Math.Floor(itemcount);
                }
                if (itemcount > 0)
                {
                    AktiveProduktion.Add(tradeitem, itemcount);
                }
                else //Um Leerlauf zu verhindern wird der Produktions Zeitslot für andere Güter zur Verfügung gestellt
                {  //Nicht ideal wegen der Sortierung in abh vom Lagerstand
                    if (proditemcnt > 1) proditemcnt--;
                }
            }
            ////////////////////////////////

            foreach (var tradeitem in AktiveProduktion.Keys)
            {
                var itemid = tradeitem.Definition;
                var useditemsdict = ItemDefinitionFactory.GetRecipeInput(itemid);
                var actprodtime = (fullprodtime * ProdRating) / proditemcnt; //Zeit die fuer dieses Item zur Verfügung steht (proditemcnt wie u.u. noch verändert)
                var neededprodtimeS = ItemDefinitionFactory.GetProductionTimeInSeconds(itemid);
                var maxprodval = actprodtime / neededprodtimeS; //Max Anzahl an
                double itemcount = AktiveProduktion[tradeitem];
                if (maxprodval < itemcount) itemcount = maxprodval;

                ////////////////// Duplikat zur anderen Schleife muss aber 2mal ausgeführt werden weil evtl das Lager schon leer ist!
                foreach (var key in useditemsdict.Keys)
                {
                    var amount = useditemsdict[key];
                    var itemneeded = Goods.Find(g => g.Definition == key);
                    if (itemneeded == null)
                    {
                        itemcount = 0;
                        break;
                    }
                    if (itemcount > (itemneeded.CurrentCargo / amount)) itemcount = itemneeded.CurrentCargo / amount;
                }
                if (!(ItemDefinitionFactory.Ores.Contains(itemid) || ItemDefinitionFactory.Ingots.Contains(itemid)))
                {
                    itemcount = Math.Floor(itemcount);
                }
                //////////////////////////////
                if (itemcount > 0)
                {
                    tradeitem.CurrentCargo += itemcount;
                    foreach (var key in useditemsdict.Keys)
                    {
                        var amount = useditemsdict[key];
                        var itemneeded = Goods.Find(g => g.Definition == key);

                        itemneeded.CurrentCargo -= (itemcount * amount);
                    }
                    //MyAPIGateway.Utilities.ShowMessage("HandleProdCycle", tradeitem.Definition.ToString() + "s: " + itemcount.ToString("0.#####") + "/" + tradeitem.CargoRatio.ToString("0.###") + "/" + neededprodtimeS + "/" + actprodtime);
                }
            }
            */
        }

        private Regex _cargoBlockRegex = new Regex(@"\((\w+):?([\w\s\\\/]*)\)", RegexOptions.Compiled & RegexOptions.IgnoreCase);
        public virtual void HandleCargos(List<IMySlimBlock> cargoblockslist)
        {
            foreach (IMySlimBlock block in cargoblockslist)
            {

                IMyCubeBlock cargoBlock = (IMyCubeBlock)block.FatBlock;
                if (cargoBlock == null) continue;

                var cname = (cargoBlock as Sandbox.ModAPI.IMyTerminalBlock).CustomData;

                if (cname == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("StationWarning:", "CustomData = null");
                    continue;
                }

                Match match = _cargoBlockRegex.Match(cname);

                if (match.Success)
                {
                    var action = match.Groups[1].Value;
                    var item = match.Groups[2].Value;
                    if (action.Equals("buy"))
                    {
                        HandleBuySequenceOnCargo(cargoBlock);
                        //MyAPIGateway.Utilities.ShowMessage("StationWarning:", "Regex buy");
                    }
                    else if (action.Equals("sell"))
                    {
                        var itemdef = ItemDefinitionFactory.DefinitionFromString(item);
                        if (itemdef != null)
                        {
                            HandleSellSequenceOnCargo(cargoBlock, itemdef);
                        }
                        //MyAPIGateway.Utilities.ShowMessage("StationWarning:", "Regex sell " + item);
                    }
                    else if (action.Equals("tobase"))
                    {
                        HandleTransferToBaseSequenceOnCargo(cargoBlock);
                    }
                }
            }
        }

        public void HandleTransferToBaseSequenceOnCargo(IMyCubeBlock cargoBlock)
        {
            //MyAPIGateway.Utilities.ShowMessage("OreListCnt", ""+ OreList.Count);
            //InventoryApi.ListItemsInventory(cargoBlock);

            InventoryApi.ListItemsInventory(cargoBlock);
            var inventory = cargoBlock.GetInventory(0);

            var items = inventory.GetItems();

            foreach (var item in items)
            {
                var itemdef = item.Content.GetId();
                var itemcount = InventoryApi.CountItemsInventory(cargoBlock, itemdef);
                if (itemcount > 0)
                {
                    InventoryApi.RemoveFromInventory(cargoBlock, itemdef, itemcount);
                }
            }
        }

        public void HandleBuySequenceOnCargo(IMyCubeBlock cargoBlock)
        {
            //MyAPIGateway.Utilities.ShowMessage("OreListCnt", ""+ OreList.Count);
            //InventoryApi.ListItemsInventory(cargoBlock);

            foreach (var tradeItem in Goods.Where(g => g.IsBuy))
            {
                var itemCount = InventoryApi.CountItemsInventory(cargoBlock, tradeItem.Definition);
                var maximumItemsPerTransfer = tradeItem.CargoSize * 0.01; //10% of max
                if (itemCount > 0)
                {
                    var pricing = tradeItem.PriceModel; //Hier sollte per pricefinder das richtige modell rausgesucht werden                                        
                    var buyPrice = pricing.GetBuyPrice(tradeItem.CargoRatio);

                    if (itemCount > maximumItemsPerTransfer) itemCount = maximumItemsPerTransfer;
                    
                    if ((itemCount + tradeItem.CurrentCargo) > tradeItem.CargoSize)
                    {
                        itemCount = (tradeItem.CargoSize - tradeItem.CurrentCargo);
                    }
                    if (!(ItemDefinitionFactory.Ores.Contains(tradeItem.Definition) || ItemDefinitionFactory.Ingots.Contains(tradeItem.Definition)))
                    {
                        itemCount = Math.Floor(itemCount);
                    }
                    //MyAPIGateway.Utilities.ShowMessage("OreCnt1", "" + itemcount);

                    var removedItemsCount = InventoryApi.RemoveFromInventory(cargoBlock, tradeItem.Definition, itemCount);
                    if (removedItemsCount != 0)
                    {
                        tradeItem.CurrentCargo += removedItemsCount;
                        //MyAPIGateway.Utilities.ShowMessage("OreCnt2", "" + removedvalue);
                        InventoryApi.AddToInventory(cargoBlock, ItemDefinitionFactory.DefinitionFromString(Definitions.Credits), (buyPrice * removedItemsCount));
                    }
                }
            }
        }

        public void HandleSellSequenceOnCargo(IMyCubeBlock cargoBlock, MyDefinitionId itemDefinition)
        {
            if (itemDefinition != null)
            {
                TradeItem tradeItem = null;
                PriceModel pricing;
                foreach (var tradeitem in Goods.Where(g => g.IsSell))
                {
                    if (tradeitem.Definition == itemDefinition)
                    {
                        tradeItem = tradeitem;
                        break;
                    }
                }
                if (tradeItem == null) return;
                var maximumItemsPerTransfer = tradeItem.CargoSize * 0.01; //10% of max

                pricing = tradeItem.PriceModel;
                var ceditsInCargo = InventoryApi.CountItemsInventory(cargoBlock, ItemDefinitionFactory.DefinitionFromString(Definitions.Credits));
                if (ceditsInCargo > 0)
                {
                    var sellPrice = pricing.GerSellPrice(tradeItem.CargoRatio);
                    var sellCount = ceditsInCargo / sellPrice;

                    if (sellCount > maximumItemsPerTransfer) sellCount = maximumItemsPerTransfer;

                    if (sellCount > tradeItem.CurrentCargo)
                    {
                        sellCount = tradeItem.CurrentCargo;
                    }

                    if (!(ItemDefinitionFactory.Ores.Contains(itemDefinition) || ItemDefinitionFactory.Ingots.Contains(itemDefinition)))
                    {
                        sellCount = Math.Floor(sellCount);
                    }


                    if ((ceditsInCargo >= (sellPrice * sellCount)) && (sellCount > 0))
                    {
                        ceditsInCargo = sellPrice * sellCount;
                        var removeCreditsFromCargo = InventoryApi.RemoveFromInventory(cargoBlock, ItemDefinitionFactory.DefinitionFromString(Definitions.Credits), ceditsInCargo);
                        if (removeCreditsFromCargo != 0)
                        {
                            if (ceditsInCargo != removeCreditsFromCargo) MyAPIGateway.Utilities.ShowMessage("DebugWarning:", "pay/remove " + ceditsInCargo.ToString("0.00##") + "/" + removeCreditsFromCargo.ToString("0.00##"));
                            InventoryApi.AddToInventory(cargoBlock, itemDefinition, sellCount);
                            tradeItem.CurrentCargo -= sellCount;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dictionary von Connectoren mit ihren verbundenen Schiffen (oder null wenn unverbunden)
        /// Dictionary [Connector -> verbundenes Schiff (kann null sein)]
        /// </summary>
        /// <param name="entity">Ein Block oder das eigene Schiff</param>
        /// <returns>Dictionary [Connector -> verbundenes Schiff (kann null sein)]</returns>
        public static Dictionary<IMyShipConnector, IMyCubeGrid> GetConnectedShips(IMyEntity entity)
        {
            var connections = new Dictionary<IMyShipConnector, IMyCubeGrid>();
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            (entity.GetTopMostParent() as IMyCubeGrid)
                .GetBlocks(blocks, slim => slim.FatBlock != null && slim.FatBlock is IMyShipConnector);

            foreach (var slim in blocks)
            {
                var connector = slim.FatBlock as IMyShipConnector;

                if (connector != null)
                {
                    if (connector.Status.Equals(Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connected))
                    {
                        connections.Add(connector, connector.OtherConnector.GetTopMostParent() as IMyCubeGrid);

                        //MyAPIGateway.Utilities.ShowMessage("skl", "connected to " + (connector.OtherConnector.GetTopMostParent() as IMyCubeGrid).DisplayName);
                    }
                    else
                    {
                        connections.Add(connector, null);
                    }

                }
            }

            return connections;
        }

        public void SetupStation(IMyTextPanel tradeBlock, bool color = false)
        {
            MyAPIGateway.Utilities.ShowMessage("TE", "Setting up Station ...");

            List<IMySlimBlock> connectors = new List<IMySlimBlock>();
            List<IMySlimBlock> cargos = new List<IMySlimBlock>();
            (tradeBlock.GetTopMostParent() as IMyCubeGrid)
                .GetBlocks(connectors, slim => slim.FatBlock != null && slim.FatBlock is IMyShipConnector);
            (tradeBlock.GetTopMostParent() as IMyCubeGrid)
                .GetBlocks(cargos, slim => slim.FatBlock != null && slim.FatBlock is IMyCargoContainer);

            if (connectors.Count == 0) MyAPIGateway.Utilities.ShowMessage("TE", "Error! No connector for cargosetup found!");
            foreach (var connector in connectors.Select(b => b.FatBlock as IMyShipConnector))
            {
                MyAPIGateway.Utilities.ShowMessage("TE", " Setting up Cargo for Connector '" + connector.CustomName + "' ...");
                //setup all cargos connected to the connector
                var connectedCargos = cargos.Select(c => c.FatBlock as IMyTerminalBlock).Where(cargo => InventoryApi.AreInventoriesConnected(connector, cargo)).ToList();

                try
                {
                    SetupConnectedCargo(connectedCargos, color);
                    MyAPIGateway.Utilities.ShowMessage("TE", " Done!");
                }
                catch (TradeEngineersException e)
                {
                    MyAPIGateway.Utilities.ShowMessage("TE", " Error: " + e.Message);
                }

            }

            if (tradeBlock.CustomName.StartsWith("SETUP:"))
                tradeBlock.CustomName = tradeBlock.CustomName.Substring(6);

            MyAPIGateway.Utilities.ShowMessage("TE", "Setup finished!");
        }

        public virtual void SetupConnectedCargo(List<IMyTerminalBlock> cargos, bool color = false)
        {
            var goodsBought = Goods.Where(g => g.IsBuy).ToList();
            var goodsSold = Goods.Where(g => g.IsSell).ToList();
            var amount = goodsBought.Count > 0 ? 1 : 0;
            amount += goodsSold.Count;

            if (cargos.Count < amount)
                throw new TradeEngineersException("Not enough attached containers (" + cargos.Count + "/" + amount + ")");

            var grid = (cargos.First().GetTopMostParent() as IMyCubeGrid);

            if (goodsBought != null && goodsBought.Count > 0)
            {
                cargos.First().CustomData = "(buy)";
                cargos.First().CustomName = "TE | Sell here";

                if (color)
                    grid.ColorBlocks(cargos.First().Position, cargos.First().Position, VRageMath.ColorExtensions.ColorToHSV(VRageMath.Color.DarkGreen));
            }
            int i = 1;

            if (goodsSold != null && goodsSold.Count > 0)
            {
                goodsSold.ForEach((gs) =>
                {
                    if (i < cargos.Count)
                    {
                        cargos[i].CustomName = "TE | Buy " + gs + " here";
                        cargos[i].CustomData = "(sell: " + gs.Definition + ")";

                        if (color)
                            grid.ColorBlocks(cargos[i].Position, cargos[i].Position, VRageMath.ColorExtensions.ColorToHSV(VRageMath.Color.Orange));
                    }

                    i++;
                });
            }


            for (; i < cargos.Count; i++)
            {
                //reset the rest to their defaults
                if (_cargoBlockRegex.IsMatch(cargos[i].CustomData))
                {
                    cargos[i].CustomData = string.Empty;
                    cargos[i].CustomName = (cargos[i] as IMyCargoContainer).DefinitionDisplayNameText;
                }
            }
        }
    }
}
