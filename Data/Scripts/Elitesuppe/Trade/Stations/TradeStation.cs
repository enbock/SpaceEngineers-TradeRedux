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
        public List<Item> Goods = new List<Item>();
        public const string StationType = "Elitesuppe_TradeRedux_TradeStation";

        public TradeStation()
        {
        }

        public TradeStation(long ownerId) : base(ownerId, StationType)
        {
            Goods = new List<Item>
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


        private readonly Regex _cargoBlockRegex = new Regex(
            @"\((\w+):?([\w\s\\\/]*)\)",
            RegexOptions.Compiled & RegexOptions.IgnoreCase
        );

        public override void HandleCargo(IEnumerable<IMySlimBlock> cargoBlockList)
        {
            foreach (IMySlimBlock block in cargoBlockList)
            {
                IMyTerminalBlock cargoBlock = block.FatBlock as IMyTerminalBlock;

                if(cargoBlock == null) continue;

                string name = cargoBlock.CustomName ?? cargoBlock.CustomNameWithFaction;
                string customData = cargoBlock.CustomData;

                if (customData == null || !name.ToLower().StartsWith("trade")) continue;

                Match match = _cargoBlockRegex.Match(customData);

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

                /*
                MyAPIGateway.Utilities.ShowMessage(
                    "HandleProdCycle",
                    tradeItem.Definition +
                    "s: " +
                    itemCount.ToString("0.#####") +
                    "/" +
                    tradeItem.CargoRatio.ToString("0.###") +
                    "/" +
                    tradeItem.CurrentCargo
                );
                */
            }
        }

        public override void TakeSettingData(StationBase oldStationData)
        {
            base.TakeSettingData(oldStationData);

            List<Item> currentGoods = Goods;
            TradeStation loadedData = (TradeStation) oldStationData;
            Goods = loadedData.Goods;
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