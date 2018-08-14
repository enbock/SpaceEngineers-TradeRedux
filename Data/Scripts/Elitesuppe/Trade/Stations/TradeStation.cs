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

namespace EliteSuppe.Trade.Stations
{
    [Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = Definitions.Version)]
    public class TradeStation : StationBase
    {
        public double ProduceFrom = 0.25f;
        public double ReduceFrom = 0.75f;
        public List<Item> Goods { get; } = new List<Item>();
        public const string StationType = "Elitesuppe_TradeRedux_TradeStation";

        public TradeStation()
        {
        }

        public TradeStation(long ownerId) : base(ownerId, StationType)
        {
            Goods = new List<Item>
            {
                new Item("MyObjectBuilder_Ingot/Gold", new Price(2f), true, true, 1000, 0),
                new Item("MyObjectBuilder_Ingot/Silver", new Price(1f), true, true, 1000, 0)
            };
        }


        private readonly Regex _cargoBlockRegex = new Regex(
            @"\((\w+):?([\w\s\\\/]*)\)",
            RegexOptions.Compiled & RegexOptions.IgnoreCase
        );

        public override void HandleCargo(IEnumerable<IMySlimBlock> cargoBlockList)
        {
            foreach (IMySlimBlock block in cargoBlockList)
            {
                IMyCubeBlock cargoBlock = block.FatBlock;
                if (cargoBlock == null) continue;

                var cargoName = (cargoBlock as IMyTerminalBlock)?.CustomData;

                if (cargoName == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("StationWarning:", "CustomData = null");
                    continue;
                }

                Match match = _cargoBlockRegex.Match(cargoName);

                if (!match.Success) continue;

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
                    try
                    {
                        var itemDefinition = ItemDefinitionFactory.DefinitionFromString(item);
                        foreach (Item tradeItem in Goods.Where(g => g.IsSell))
                        {
                            if (tradeItem.Definition != itemDefinition) continue;
                            HandleSellSequenceOnCargo(cargoBlock, tradeItem);
                        }
                    }
                    catch (UnknownItemException exception)
                    {
                        MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                    }
                }
            }
        }

        public override void HandleProdCycle()
        {
            IEnumerable<Item> productionItems =
                Goods.Where(good => good.CargoRatio < ProduceFrom || good.CargoRatio > ReduceFrom);

            foreach (Item tradeItem in productionItems)
            {
                bool sell = false;
                MyDefinitionId itemId = tradeItem.Definition;
                double itemCount = 0f;

                if (tradeItem.CargoRatio > ReduceFrom)
                {
                    itemCount = -1f * (tradeItem.CargoSize * 0.01f);
                }

                if (tradeItem.CargoRatio < ProduceFrom)
                {
                    itemCount = tradeItem.CargoSize * 0.01f;
                }

                if (itemCount < 0)
                {
                    itemCount = Math.Abs(itemCount);
                    sell = true;
                }

                if (itemCount > tradeItem.CargoSize) itemCount = tradeItem.CargoSize;

                itemCount = Math.Floor(itemCount);

                if (itemCount > 0)
                {
                    if (sell)
                    {
                        tradeItem.CurrentCargo -= itemCount;
                        var sellPrice = tradeItem.Price.GetSellPrice(tradeItem.CargoRatio);
                    }
                    else
                    {
                        tradeItem.CurrentCargo += itemCount;
                        var buyPrice = tradeItem.Price.GetBuyPrice(tradeItem.CargoRatio);
                    }
                }

                MyAPIGateway.Utilities.ShowMessage("HandleProdCycle",
                    tradeItem.Definition + "s: " + itemCount.ToString("0.#####") + "/" +
                    tradeItem.CargoRatio.ToString("0.###") + "/" + tradeItem.CurrentCargo);
            }
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            base.TakeSettingData(oldStationData);
            foreach (Item beforeItem in ((TradeStation)oldStationData).Goods)
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