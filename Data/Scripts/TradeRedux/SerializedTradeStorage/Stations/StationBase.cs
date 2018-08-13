using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using TradeRedux.Exceptions;
using TradeRedux.Inventory;
using TradeRedux.PluginApi;
using TradeRedux.TradeGoods;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using IMyCargoContainer = Sandbox.ModAPI.IMyCargoContainer;
using IMyShipConnector = Sandbox.ModAPI.IMyShipConnector;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;
using IMyTextPanel = Sandbox.ModAPI.IMyTextPanel;

namespace TradeRedux.SerializedTradeStorage.Stations
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.DataFormat)]
    public abstract class StationBase
    {
        public static string StationType = "StationBaseType";
        public string Type { get; } = "";

        public double BaseCargoSize { get; set; } = 1000;
        public List<TradeItem> Goods { get; } = new List<TradeItem>();

        public long OwnerId
        {
            get { return _ownerId; }
        }

        protected long _ownerId = 0;

        public StationBase()
        {
        }

        public StationBase(long ownerId, string type)
        {
            _ownerId = ownerId;
            Type = type;
        }

        public static StationBase Factory(string blockName, long ownerId)
        {
            if (string.IsNullOrWhiteSpace(blockName))
                throw new ArgumentException("Station Block name was empty");
            string name = blockName.ToUpper();

            if (TradeStation.StationType.ToUpper() == name)
                return new TradeStation(true, ownerId,
                    TradeStation.StationType);

            throw new ArgumentException(
                "Station Block name did not match a station kind");
        }

        public virtual void HandleProdCycle(double fullprodtime)
        {
        }

        private Regex _cargoBlockRegex = new Regex(@"\((\w+):?([\w\s\\\/]*)\)",
            RegexOptions.Compiled & RegexOptions.IgnoreCase);

        public virtual void HandleCargos(List<IMySlimBlock> cargoblockslist)
        {
            foreach (IMySlimBlock block in cargoblockslist)
            {
                IMyCubeBlock cargoBlock = (IMyCubeBlock) block.FatBlock;
                if (cargoBlock == null) continue;

                var cname = (cargoBlock as IMyTerminalBlock) .CustomData;

                if (cname == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("StationWarning:",
                        "CustomData = null");
                    continue;
                }

                Match match = _cargoBlockRegex.Match(cname);

                if (match.Success)
                {
                    var action = match.Groups[1].Value;
                    var item = match.Groups[2].Value;
                    if (action.Equals("buy"))
                    {
                        foreach (var tradeItem in Goods.Where(g => g.IsBuy))
                        {
                            HandleBuySequenceOnCargo(cargoBlock, tradeItem);
                        }
                    }
                    else if (action.Equals("sell"))
                    {
                        MyDefinitionId itemDefinition;
                        try
                        {
                            itemDefinition =
                                ItemDefinitionFactory
                                    .DefinitionFromString(item);
                            if (itemDefinition != null)
                            {
                                foreach (TradeItem tradeItem in Goods.Where(g =>
                                    g.IsSell))
                                {
                                    if (tradeItem.Definition != itemDefinition)
                                        continue;
                                    HandleSellSequenceOnCargo(cargoBlock,
                                        tradeItem);
                                }
                            }
                        }
                        catch (UnknownItemException exception)
                        {
                            MyAPIGateway.Utilities.ShowMessage("Error",
                                "Wrong item: " + exception.Message);
                        }
                    }
                }
            }
        }

        public virtual void TakeSettingData(StationBase oldStationData)
        {
        }

        public void HandleBuySequenceOnCargo(IMyCubeBlock cargoBlock,
            TradeItem tradeItem)
        {
            var itemCount =
                InventoryApi.CountItemsInventory(cargoBlock,
                    tradeItem.Definition);
            var maximumItemsPerTransfer =
                tradeItem.CargoSize * 0.01; //10% of max
            if (itemCount > 0)
            {
                var pricing =
                    tradeItem
                        .PriceModel; //Hier sollte per pricefinder das richtige modell rausgesucht werden                                        
                var buyPrice = pricing.GetBuyPrice(tradeItem.CargoRatio);

                if (itemCount > maximumItemsPerTransfer)
                    itemCount = maximumItemsPerTransfer;

                if ((itemCount + tradeItem.CurrentCargo) > tradeItem.CargoSize)
                {
                    itemCount = (tradeItem.CargoSize - tradeItem.CurrentCargo);
                }

                itemCount = Math.Floor(itemCount);
                //MyAPIGateway.Utilities.ShowMessage("OreCnt1", "" + itemcount);

                var removedItemsCount =
                    InventoryApi.RemoveFromInventory(cargoBlock,
                        tradeItem.Definition, itemCount);
                if (removedItemsCount != 0)
                {
                    tradeItem.CurrentCargo += removedItemsCount;
                    //MyAPIGateway.Utilities.ShowMessage("OreCnt2", "" + removedvalue);
                    try
                    {
                        double paymentAmount =
                            Math.Floor(buyPrice * removedItemsCount);
                        InventoryApi.AddToInventory(cargoBlock,
                            ItemDefinitionFactory.DefinitionFromString(
                                Definitions.Credits), paymentAmount);

                        Logger.Log("Buy:" + itemCount + "  Payed Credits:" +
                                   paymentAmount + "   Taken item: " +
                                   removedItemsCount);
                    }
                    catch (UnknownItemException)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                    }
                }
            }
        }

        public void HandleSellSequenceOnCargo(IMyCubeBlock cargoBlock,
            TradeItem tradeItem)
        {
            PriceModel pricing = tradeItem.PriceModel;
            MyDefinitionId itemDefinition = tradeItem.Definition;

            double maximumItemsPerTransfer =
                tradeItem.CargoSize * 0.01f; //10% of max

            double ceditsInCargo = InventoryApi.CountItemsInventory(cargoBlock,
                ItemDefinitionFactory
                    .DefinitionFromString(Definitions.Credits));
            if (ceditsInCargo > 0)
            {
                double sellPrice = pricing.GerSellPrice(tradeItem.CargoRatio);
                double sellCount = ceditsInCargo / sellPrice;

                if (sellCount > maximumItemsPerTransfer)
                    sellCount = maximumItemsPerTransfer;

                if (sellCount > tradeItem.CurrentCargo)
                {
                    sellCount = tradeItem.CurrentCargo;
                }

                sellCount = Math.Floor(sellCount);
                double paymentAmount = Math.Ceiling(sellPrice * sellCount);

                if (ceditsInCargo >= paymentAmount && (sellCount > 0))
                {
                    try
                    {
                        double removeCreditsFromCargo =
                            InventoryApi.RemoveFromInventory(cargoBlock,
                                ItemDefinitionFactory.DefinitionFromString(
                                    Definitions.Credits), paymentAmount);
                        Logger.Log("Sellcount:" + sellCount + "  Credits:" +
                                   ceditsInCargo + "   Taken credits: " +
                                   removeCreditsFromCargo);

                        if (removeCreditsFromCargo != 0)
                        {
                            if (paymentAmount != removeCreditsFromCargo)
                                Logger.Log(
                                    "pay/remove " +
                                    ceditsInCargo.ToString("0.00##") + "/" +
                                    removeCreditsFromCargo.ToString("0.00##"));
                            InventoryApi.AddToInventory(cargoBlock,
                                itemDefinition, sellCount);
                            tradeItem.CurrentCargo -= sellCount;
                        }
                    }
                    catch (UnknownItemException)
                    {
                        //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                    }
                }
            }
        }

        public static Dictionary<IMyShipConnector, IMyCubeGrid>
            GetConnectedShips(IMyEntity entity)
        {
            var connections = new Dictionary<IMyShipConnector, IMyCubeGrid>();
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            (entity.GetTopMostParent() as IMyCubeGrid)
                .GetBlocks(blocks,
                    slim => slim.FatBlock != null &&
                            slim.FatBlock is IMyShipConnector);

            foreach (var slim in blocks)
            {
                var connector = slim.FatBlock as IMyShipConnector;

                if (connector != null)
                {
                    if (connector.Status.Equals(MyShipConnectorStatus.Connected))
                    {
                        connections.Add(connector,
                            connector.OtherConnector.GetTopMostParent() as
                                IMyCubeGrid);

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
                .GetBlocks(connectors,
                    slim => slim.FatBlock != null &&
                            slim.FatBlock is IMyShipConnector);
            (tradeBlock.GetTopMostParent() as IMyCubeGrid)
                .GetBlocks(cargos,
                    slim => slim.FatBlock != null &&
                            slim.FatBlock is IMyCargoContainer);

            if (connectors.Count == 0)
                MyAPIGateway.Utilities.ShowMessage("TE",
                    "Error! No connector for cargosetup found!");
            foreach (var connector in connectors.Select(b =>
                b.FatBlock as IMyShipConnector))
            {
                MyAPIGateway.Utilities.ShowMessage("TE",
                    " Setting up Cargo for Connector '" + connector.CustomName +
                    "' ...");
                //setup all cargos connected to the connector
                var connectedCargos = cargos
                    .Select(c => c.FatBlock as IMyTerminalBlock).Where(cargo =>
                        InventoryApi.AreInventoriesConnected(connector, cargo))
                    .ToList();

                try
                {
                    SetupConnectedCargo(connectedCargos, color);
                    MyAPIGateway.Utilities.ShowMessage("TE", " Done!");
                }
                catch (TradeEngineersException e)
                {
                    MyAPIGateway.Utilities.ShowMessage("TE",
                        " Error: " + e.Message);
                }
            }

            if (tradeBlock.CustomName.StartsWith("SETUP:"))
                tradeBlock.CustomName = tradeBlock.CustomName.Substring(6);

            MyAPIGateway.Utilities.ShowMessage("TE", "Setup finished!");
        }

        public virtual void SetupConnectedCargo(List<IMyTerminalBlock> cargos,
            bool color = false)
        {
            var goodsBought = Goods.Where(g => g.IsBuy).ToList();
            var goodsSold = Goods.Where(g => g.IsSell).ToList();
            var amount = goodsBought.Count > 0 ? 1 : 0;
            amount += goodsSold.Count;

            if (cargos.Count < amount)
                throw new TradeEngineersException(
                    "Not enough attached containers (" + cargos.Count + "/" +
                    amount + ")");

            var grid = (cargos.First().GetTopMostParent() as IMyCubeGrid);

            if (goodsBought != null && goodsBought.Count > 0)
            {
                cargos.First().CustomData = "(buy)";
                cargos.First().CustomName = "TE | Sell here";

                if (color)
                    grid.ColorBlocks(cargos.First().Position,
                        cargos.First().Position,
                        ColorExtensions.ColorToHSV(Color
                            .DarkGreen));
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
                            grid.ColorBlocks(cargos[i].Position,
                                cargos[i].Position,
                                ColorExtensions.ColorToHSV(Color.Orange));
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
                    cargos[i].CustomName = (cargos[i] as IMyCargoContainer)
                        .DefinitionDisplayNameText;
                }
            }
        }
    }
}