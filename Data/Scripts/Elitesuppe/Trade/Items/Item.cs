using System;
using Elitesuppe.Trade.Exceptions;
using Elitesuppe.Trade.Inventory;
using Sandbox.ModAPI;
using VRage.Game;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Item
    {
        public Price SellPrice;
        public Price PurchasePrice;

        public bool IsSelling
        {
            get { return SellPrice.Amount > 0; }
        }

        public bool IsPurchasing
        {
            get { return PurchasePrice.Amount > 0; }
        }

        public double CurrentCargo = 0f;
        public double CargoSize = double.MaxValue;
        public double Required = 0f;
        public double Result = 0f;
        private MyDefinitionId _definition;

        public MyDefinitionId Definition
        {
            get { return _definition; }
        }

        public string SerializedDefinition
        {
            get { return Definition.ToString().Replace("MyObjectBuilder_", ""); }

            // Used for XML to Object encoding
            set { ParseDefinition(value); }
        }

        public double CargoRatio
        {
            get { return 1f / CargoSize * CurrentCargo; }
        }

        public Item()
        {
        }

        public Item(string itemType)
        {
            ParseDefinition(itemType);
            PurchasePrice = new Price(1f, 1f, 1f);
            SellPrice = new Price(1f, 1f, 1f);
            CargoSize = 0;
            CurrentCargo = 0;
        }

        public Item(
            string itemType,
            Price purchasePrice,
            Price sellPrice,
            double cargoSize = 1000f,
            double currentCargo = 0f
        )
        {
            ParseDefinition(itemType);
            PurchasePrice = purchasePrice;
            SellPrice = sellPrice;
            CargoSize = cargoSize;
            CurrentCargo = currentCargo;
        }

        public Item(
            string itemType,
            Price purchasePrice,
            Price sellPrice,
            double required = 0f,
            double result = 0f,
            double cargoSize = 1000f,
            double currentCargo = 0f
        )
        {
            ParseDefinition(itemType);
            PurchasePrice = purchasePrice;
            SellPrice = sellPrice;
            CargoSize = cargoSize;
            CurrentCargo = currentCargo;
            Required = required;
            Result = result;
        }

        private void ParseDefinition(string itemType)
        {
            try
            {
                _definition = ItemDefinitionFactory.DefinitionFromString(itemType);
            }
            catch (UnknownItemException exception)
            {
                MyAPIGateway.Utilities.ShowNotification("Error: Creating item: " + exception.Message);
            }
        }

        public override string ToString()
        {
            return ItemDefinitionFactory.DefinitionToString(Definition);
        }

        public Item Clone()
        {
            Item copy = MemberwiseClone() as Item;
            // ReSharper disable once PossibleNullReferenceException
            copy.SellPrice = SellPrice.Clone();
            copy.PurchasePrice = PurchasePrice.Clone();

            return copy;
        }
    }
}