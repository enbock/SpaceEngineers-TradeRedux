using System;
using Elitesuppe.Trade.Exceptions;
using Elitesuppe.Trade.Inventory;
using VRage.Game;

namespace EliteSuppe.Trade.Items
{
    [Serializable]
    public class Item
    {
        public Item()
        {
        }

        public Item(
            string itemType,
            Price price,
            bool sell,
            bool buy,
            double cargoSize = 1000,
            double currentCargo = 500
        )
        {
            try
            {
                _definition = ItemDefinitionFactory.DefinitionFromString(itemType);
                Price = price;
                CargoSize = cargoSize;
                CurrentCargo = currentCargo;
                IsSell = sell;
                IsBuy = buy;
            }
            catch (UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
        }

        public Item(
            string itemType,
            Price price,
            double required = 0f,
            double result = 0f,
            double cargoSize = 1000,
            double currentCargo = 500
        )
        {
            try
            {
                _definition = ItemDefinitionFactory.DefinitionFromString(itemType);
                Price = price;
                CargoSize = cargoSize;
                CurrentCargo = currentCargo;
                Required = required;
                Result = result;
                IsSell = false;
                IsBuy = false;
            }
            catch (UnknownItemException)
            {
                //MyAPIGateway.Utilities.ShowMessage("Error", "Wrong item: " + exception.Message);
            }
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
                    _definition = ItemDefinitionFactory.DefinitionFromString(value);
                }
                catch (UnknownItemException)
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
            return ItemDefinitionFactory.DefinitionToString(Definition);
        }
    }
}