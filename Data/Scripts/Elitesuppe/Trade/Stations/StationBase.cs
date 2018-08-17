using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elitesuppe.Trade;
using Elitesuppe.Trade.Exceptions;
using Elitesuppe.Trade.Inventory;
using EliteSuppe.Trade.Items;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.Version)]
    public abstract class StationBase
    {
        public string Type { get; } = "StationBaseType";
        public long OwnerId { get; } = 0;

        protected StationBase()
        {
        }

        protected StationBase(long ownerId, string type)
        {
            OwnerId = ownerId;
            Type = type;
        }

        public static StationBase Factory(string blockName, long ownerId)
        {
            if (string.IsNullOrWhiteSpace(blockName)) throw new ArgumentException("Station Block name was empty");

            switch (blockName)
            {
                case TradeStation.StationType:
                    return new TradeStation(ownerId);
                case IronForge.StationType:
                    return new IronForge(ownerId);
                case MiningStation.StationType:
                    return new MiningStation(ownerId);
            }

            throw new ArgumentException("Station Block name did not match a station kind");
        }

        public virtual void HandleProdCycle()
        {
        }

        public virtual void HandleCargo(IEnumerable<IMySlimBlock> cargoBlockList)
        {
        }

        public virtual void TakeSettingData(StationBase oldStationData)
        {
        }

        public virtual void HandleBuySequenceOnCargo(IMyCubeBlock cargoBlock, Item item)
        {
            double availableCount = InventoryApi.CountItemsInventory(cargoBlock, item.Definition);
            double itemCount = availableCount;
            double maximumItemsPerTransfer = item.CargoSize * 0.01;
            if (!(itemCount > 0)) return;
            var pricing = item.Price;
            var buyPrice = pricing.GetBuyPrice(item.CargoRatio);
            double minimumItemPerTransfer = (1f / buyPrice) * 2f;
            if (maximumItemsPerTransfer < minimumItemPerTransfer * 5f)
            {
                maximumItemsPerTransfer = minimumItemPerTransfer * 5f;
            }

            if (itemCount > maximumItemsPerTransfer) itemCount = maximumItemsPerTransfer;

            if ((itemCount + item.CurrentCargo) > item.CargoSize)
            {
                itemCount = (item.CargoSize - item.CurrentCargo);
            }

            itemCount = Math.Floor(itemCount);

            if (itemCount < minimumItemPerTransfer)
            {
                if (minimumItemPerTransfer > availableCount) return;
                
                itemCount = minimumItemPerTransfer;
            }

            var removedItemsCount = InventoryApi.RemoveFromInventory(cargoBlock, item.Definition, itemCount);
            if (removedItemsCount <= 0) return;
            item.CurrentCargo += removedItemsCount;
            try
            {
                double paymentAmount = Math.Floor(buyPrice * removedItemsCount);
                InventoryApi.AddToInventory(
                    cargoBlock,
                    ItemDefinitionFactory.DefinitionFromString(Definitions.Credits),
                    paymentAmount
                );

                // Logger.Log("Buy:" + itemCount + "  Payed Credits:" + paymentAmount + "   Taken item: " + removedItemsCount);
            }
            catch (UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        public virtual void HandleSellSequenceOnCargo(IMyCubeBlock cargoBlock, Item item)
        {
            Price pricing = item.Price;
            MyDefinitionId itemDefinition = item.Definition;

            double maximumItemsPerTransfer = item.CargoSize * 0.01f; //10% of max

            double creditsInCargo = InventoryApi.CountItemsInventory(
                cargoBlock,
                ItemDefinitionFactory.DefinitionFromString(Definitions.Credits)
            );
            if (!(creditsInCargo > 0)) return;
            double sellPrice = pricing.GetSellPrice(item.CargoRatio);
            double sellCount = creditsInCargo / sellPrice;

            if (sellCount > maximumItemsPerTransfer) sellCount = maximumItemsPerTransfer;

            if (sellCount > item.CurrentCargo)
            {
                sellCount = item.CurrentCargo;
            }

            sellCount = Math.Floor(sellCount);
            double paymentAmount = Math.Ceiling(sellPrice * sellCount);

            if (creditsInCargo >= paymentAmount && (sellCount > 0))
            {
                try
                {
                    double removeCreditsFromCargo = InventoryApi.RemoveFromInventory(
                        cargoBlock,
                        ItemDefinitionFactory.DefinitionFromString(Definitions.Credits),
                        paymentAmount
                    );
                    // Logger.Log("Sellcount:" + sellCount + "  Credits:" + ceditsInCargo + "   Taken credits: " + removeCreditsFromCargo);

                    if (!(Math.Abs(removeCreditsFromCargo) > 0)) return;
                    // if (paymentAmount != removeCreditsFromCargo) Logger.Log("pay/remove " + ceditsInCargo.ToString("0.00##") + "/" + removeCreditsFromCargo.ToString("0.00##"));
                    InventoryApi.AddToInventory(cargoBlock, itemDefinition, sellCount);
                    item.CurrentCargo -= sellCount;
                }
                catch (UnknownItemException)
                {
                    //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                }
            }
        }

        public static Dictionary<IMyShipConnector, IMyCubeGrid> GetConnectedShips(IMyEntity entity)
        {
            var connections = new Dictionary<IMyShipConnector, IMyCubeGrid>();
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            (entity.GetTopMostParent() as IMyCubeGrid)?.GetBlocks(
                blocks,
                slim => slim.FatBlock is IMyShipConnector
            );

            foreach (var slim in blocks)
            {
                IMyShipConnector connector = slim.FatBlock as IMyShipConnector;
                if (!(slim.FatBlock is IMyShipConnector)) continue;
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

            return connections;
        }


        /*
        public void SetupStation(IMyTextPanel tradeBlock, bool color = false)
        {
            MyAPIGateway.Utilities.ShowMessage("TE", "Setting up Station ...");

            List<IMySlimBlock> connectors = new List<IMySlimBlock>();
            List<IMySlimBlock> cargoList = new List<IMySlimBlock>();
            (tradeBlock.GetTopMostParent() as IMyCubeGrid)?.GetBlocks(
                connectors,
                slim => slim.FatBlock != null && slim.FatBlock is IMyShipConnector
            );
            (tradeBlock.GetTopMostParent() as IMyCubeGrid)?.GetBlocks(
                cargoList,
                slim => slim.FatBlock != null && slim.FatBlock is IMyCargoContainer
            );

            if (connectors.Count == 0)
                MyAPIGateway.Utilities.ShowMessage("TE", "Error! No connector for cargosetup found!");
            foreach (var connector in connectors.Select(b => b.FatBlock as IMyShipConnector))
            {
                if (connector == null) continue;
                MyAPIGateway.Utilities.ShowMessage(
                    "TE",
                    " Setting up Cargo for Connector '" + connector.CustomName + "' ..."
                );
                //setup all cargo connected to the connector
                var connectedCargoList = cargoList.Select(c => c.FatBlock as IMyTerminalBlock)
                    .Where(cargo => InventoryApi.AreInventoriesConnected(connector, cargo))
                    .ToList();

                try
                {
                    SetupConnectedCargo(connectedCargoList, color);
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
            List<Item> goodsBought = Goods.Where(g => g.IsBuy).ToList();
            List<Item> goodsSold = Goods.Where(g => g.IsSell).ToList();
            double amount = goodsBought.Count > 0 ? 1 : 0;
            amount += goodsSold.Count;

            if (cargos.Count < amount)
                throw new TradeEngineersException(
                    "Not enough attached containers (" + cargos.Count + "/" + amount + ")"
                );

            var grid = (cargos.First().GetTopMostParent() as IMyCubeGrid);

            if (goodsBought.Count > 0)
            {
                cargos.First().CustomData = "(buy)";
                cargos.First().CustomName = "TE | Sell here";

                if (color)
                    grid.ColorBlocks(
                        cargos.First().Position,
                        cargos.First().Position,
                        VRageMath.ColorExtensions.ColorToHSV(VRageMath.Color.DarkGreen)
                    );
            }

            int i = 1;

            if (goodsSold.Count > 0)
            {
                goodsSold.ForEach(
                    (gs) =>
                    {
                        if (i < cargos.Count)
                        {
                            cargos[i].CustomName = "TE | Buy " + gs + " here";
                            cargos[i].CustomData = "(sell: " + gs.Definition + ")";

                            if (color)
                            {
                                grid?.ColorBlocks(
                                    cargos[i].Position,
                                    cargos[i].Position,
                                    VRageMath.ColorExtensions.ColorToHSV(VRageMath.Color.Orange)
                                );
                            }
                        }

                        i++;
                    }
                );
            }


            for (; i < cargos.Count; i++)
            {
                //reset the rest to their defaults
                if (!_cargoBlockRegex.IsMatch(cargos[i].CustomData)) continue;
                cargos[i].CustomData = string.Empty;
                cargos[i].CustomName = (cargos[i] as IMyCargoContainer)?.DefinitionDisplayNameText;
            }
        }
        */
    }
}