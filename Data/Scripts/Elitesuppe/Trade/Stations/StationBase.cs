using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using Elitesuppe.Trade.Exceptions;
using Elitesuppe.Trade.Inventory;
using EliteSuppe.Trade.Items;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using IMyShipConnector = Sandbox.ModAPI.IMyShipConnector;

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [XmlRoot(Namespace = Definitions.Version)]
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

        public virtual void HandleBuySequenceOnCargo(IMyCubeBlock cargoBlock, Item item, Item credit = null)
        {
            if (credit == null)
            {
                credit = new Item(Definitions.Credits, new Price(1f, 1f, 1f), true, true, 0f, 0f);
            }

            MyDefinitionId creditDefinition = credit.Definition;

            double availableCount = InventoryApi.CountItemsInventory(cargoBlock, item.Definition);
            if (availableCount <= 0) return;

            double itemCount = availableCount;
            double maximumItemsPerTransfer = Math.Ceiling(item.CargoSize * 0.01f);
            var pricing = item.Price;
            var buyPrice = pricing.GetBuyPrice(item.CargoRatio);
            double minimumItemPerTransfer = Math.Ceiling(1f / buyPrice);
            if (maximumItemsPerTransfer < minimumItemPerTransfer)
            {
                maximumItemsPerTransfer = minimumItemPerTransfer;
            }

            if (itemCount > maximumItemsPerTransfer) itemCount = maximumItemsPerTransfer;

            if (itemCount + item.CurrentCargo > item.CargoSize)
            {
                itemCount = item.CargoSize - item.CurrentCargo;
            }

            itemCount = Math.Floor(itemCount);

            if (itemCount < minimumItemPerTransfer)
            {
                if (minimumItemPerTransfer > availableCount) return;

                itemCount = minimumItemPerTransfer;
            }

            double paymentAmount = Math.Floor(buyPrice * itemCount);
            if (IsCreditLimitationEnabled(credit) && paymentAmount > credit.CurrentCargo)
            {
                paymentAmount = credit.CargoSize;
                itemCount = Math.Ceiling(paymentAmount / buyPrice);
            }

            var removedItemsCount =
                Math.Floor(InventoryApi.RemoveFromInventory(cargoBlock, item.Definition, itemCount));
            if (removedItemsCount <= 0) return;

            // could less items removed as expected
            if (removedItemsCount < itemCount) paymentAmount = Math.Floor(buyPrice * removedItemsCount);

            item.CurrentCargo += removedItemsCount;
            try
            {
                double givenCredits = InventoryApi.AddToInventory(cargoBlock, creditDefinition, paymentAmount);

                if (givenCredits <= 0)
                {
                    // rollback
                    item.CurrentCargo -= removedItemsCount;
                    InventoryApi.AddToInventory(cargoBlock, item.Definition, removedItemsCount);
                    return;
                }

                if (IsCreditLimitationEnabled(credit))
                {
                    credit.CurrentCargo -= givenCredits;
                }
            }
            catch (UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        public virtual void HandleSellSequenceOnCargo(IMyCubeBlock cargoBlock, Item item, Item credit = null)
        {
            if (credit == null)
            {
                credit = new Item(Definitions.Credits, new Price(1f, 1f, 1f), true, true, 0f, 0f);
            }

            MyDefinitionId creditDefinition = credit.Definition;

            double sellPrice = item.Price.GetSellPrice(item.CargoRatio);
            MyDefinitionId itemDefinition = item.Definition;

            double maximumItemsPerTransfer = Math.Round(item.CargoSize * 0.01f, 0); //1% of max
            if (maximumItemsPerTransfer < 1f) maximumItemsPerTransfer = 1f;

            double creditsInCargo = InventoryApi.CountItemsInventory(cargoBlock, creditDefinition);
            if (creditsInCargo <= 1f) return;

            double sellCount = Math.Floor(creditsInCargo / sellPrice);

            if (sellCount < 1f) return;

            if (sellCount > maximumItemsPerTransfer) sellCount = maximumItemsPerTransfer;
            if (sellCount > item.CurrentCargo) sellCount = item.CurrentCargo;

            double paymentAmount = Math.Ceiling(sellPrice * sellCount);

            if (paymentAmount > creditsInCargo) return;

            try
            {
                double payedCredits = InventoryApi.RemoveFromInventory(cargoBlock, creditDefinition, paymentAmount);

                if (payedCredits <= 0) return;

                item.CurrentCargo -= sellCount;
                double selledItems = InventoryApi.AddToInventory(cargoBlock, itemDefinition, sellCount);

                if (selledItems <= 0)
                {
                    // rollback if nothing given
                    item.CurrentCargo += sellCount;
                    InventoryApi.AddToInventory(cargoBlock, creditDefinition, paymentAmount);
                    return;
                }

                if (IsCreditLimitationEnabled(credit))
                {
                    credit.CurrentCargo += payedCredits;
                }
            }
            catch (UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        private static bool IsCreditLimitationEnabled(Item credit)
        {
            return credit.CargoSize > 0;
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
                if (connector.Status.Equals(MyShipConnectorStatus.Connected))
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