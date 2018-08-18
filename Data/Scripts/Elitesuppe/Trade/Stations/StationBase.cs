using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Elitesuppe.Trade;
using Elitesuppe.Trade.Exceptions;
using Elitesuppe.Trade.Inventory;
using EliteSuppe.Trade.Items;
using VRage.Game;
using VRage.Game.ModAPI;

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
    }
}