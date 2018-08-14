using System;
using VRage.Game;
using Sandbox.ModAPI;
using Elitesuppe.Trade.TradeGoods;

namespace Elitesuppe.Trade.Serialized.Items
{
    [Serializable]
    public class Item
    {
        public Item()
        {
        }

        public Item(
            MyDefinitionId itemType,
            Price price,
            bool sell,
            bool buy,
            int cargoSize = 1000,
            int currentCargo = 500
        )
        {
            _definition = itemType;
            Price = price;
            CargoSize = cargoSize;
            CurrentCargo = currentCargo;
            IsSell = sell;
            IsBuy = buy;
        }

        public Item(
            string itemType,
            Price price,
            bool sell,
            bool buy,
            int cargoSize = 1000,
            int currentCargo = 500
        )
        {
            try
            {
                _definition = Inventory.ItemDefinitionFactory.DefinitionFromString(itemType);
                Price = price;
                CargoSize = cargoSize;
                CurrentCargo = currentCargo;
                IsSell = sell;
                IsBuy = buy;
            }
            catch (Exceptions.UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        public Item(
            string itemType,
            Price price,
            double required = 0f,
            double result = 0f,
            int cargoSize = 1000,
            int currentCargo = 500
        )
        {
            _definition = Inventory.ItemDefinitionFactory.DefinitionFromString(itemType);
            Price = price;
            CargoSize = cargoSize;
            CurrentCargo = currentCargo;
            Required = required;
            Result = result;
            IsSell = false;
            IsBuy = false;
        }

        private MyDefinitionId _definition;

        public MyDefinitionId Definition
        {
            get { return _definition; }
        }

        public string SerializedDefinition
        {
            get { return Definition.ToString(); }

            set
            {
                try
                {
                    _definition = Inventory.ItemDefinitionFactory.DefinitionFromString(value);
                }
                catch (Exceptions.UnknownItemException)
                {
                    //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
                }
            }
        }

        public bool IsSell { get; set; }

        public bool IsBuy { get; set; }

        public double CurrentCargo { get; set; }

        public double CargoSize { get; set; } = double.MaxValue;

        public double Required { get; set; } = 0;

        public double Result { get; set; } = 0;

        public double CargoRatio
        {
            get { return 1f / CargoSize * CurrentCargo; }
        }

        public Price Price = new Price(1.0);

        public override string ToString()
        {
            return Inventory.ItemDefinitionFactory.DefinitionToString(Definition);
        }
    }
}